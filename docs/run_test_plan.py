"""
Runs docs/scalar-test-plan.json against a live API and reports pass/fail per step.

This is the same execution model as the generated Postman collection - same variable
substitution, same capture rules, same status assertions - driven from Python so you do
not need Postman or a browser.

It additionally solves the two things a human cannot script around:

  * OTPs. They normally arrive by e-mail. Here the stored HMAC is brute-forced locally
    (the code is 6 digits and the hashing secret is in your own user-secrets), so the
    verify steps run for real instead of being skipped.
  * Invitation tokens. S070 returns the token as a bare GUID in "data", so it is captured
    from the response rather than re-read from the database.

Usage:
    python docs/run_test_plan.py
    python docs/run_test_plan.py --only S100 S101        # run a subset
    python docs/run_test_plan.py --skip S040 S041        # skip rate-limit-heavy steps
"""
from __future__ import annotations

import argparse
import base64
import hashlib
import hmac
import json
import os
import pathlib
import re
import ssl
import subprocess
import sys
import time
import urllib.error
import urllib.parse
import urllib.request
import uuid

ROOT = pathlib.Path(__file__).resolve().parent.parent
PLAN_PATH = ROOT / "docs" / "scalar-test-plan.json"
CSPROJ = ROOT / "src" / "Skill-Loop.Api" / "Skill-Loop.Api.csproj"
DB = "Skill-Loop"
API_USER_SECRETS_ID = None

TOKEN_VAR = {
    "adminToken": "adminToken",
    "userToken": "userToken",
    "user2Token": "user2Token",
    "instructorToken": "instructorToken",
    "devAdminToken": "devAdminToken",
}

PATHY = re.compile(r"^[A-Za-z]:[\\/]|^/|^\./")
TEMPLATE = re.compile(r"\{\{([^}]+)\}\}")


# --------------------------------------------------------------------- database
def sql(query: str) -> list[list[str]]:
    """Run a query through sqlcmd and return rows as lists of strings.

    `-b` makes sqlcmd exit non-zero on a SQL error. Without it sqlcmd happily reports
    "Msg 208, Level=Error" on stdout and still exits 0, which previously let an error
    message be captured as if it were a GUID.
    """
    proc = subprocess.run(
        ["sqlcmd", "-S", "localhost", "-d", DB, "-E", "-C", "-b", "-h", "-1", "-W", "-Q", "SET NOCOUNT ON; " + query],
        capture_output=True, text=True, timeout=60,
    )
    out = proc.stdout or ""
    if proc.returncode != 0 or re.search(r"Msg \d+, Level=", out):
        raise RuntimeError((proc.stderr.strip() or out.strip() or "sqlcmd failed"))
    rows = []
    for line in out.splitlines():
        line = line.strip()
        if line:
            rows.append([c.strip() for c in line.split("  ") if c.strip()])
    return rows


def user_secrets() -> dict:
    """Read .NET user-secrets for the API project."""
    global API_USER_SECRETS_ID
    text = CSPROJ.read_text(encoding="utf-8")
    m = re.search(r"<UserSecretsId>([^<]+)</UserSecretsId>", text)
    if not m:
        return {}
    API_USER_SECRETS_ID = m.group(1)
    p = pathlib.Path.home() / "AppData" / "Roaming" / "Microsoft" / "UserSecrets" / API_USER_SECRETS_ID / "secrets.json"
    if not p.exists():
        return {}
    # user-secrets.json is written with a BOM by the .NET tooling.
    return json.loads(p.read_text(encoding="utf-8-sig"))


# ------------------------------------------------------------------------- OTP
def load_dotenv() -> dict:
    """Parse the repo-root .env the way docs/run-api-local.ps1 does."""
    p = ROOT / ".env"
    vals = {}
    if not p.exists():
        return vals
    for line in p.read_text(encoding="utf-8", errors="replace").splitlines():
        m = re.match(r"^\s*([^#=\s]+)\s*=\s*(.*)$", line)
        if m:
            vals[m.group(1).strip()] = m.group(2).strip().strip('"').strip("'")
    return vals


def app_setting(dotenv: dict, secrets: dict, env_key: str, secret_key: str) -> str:
    """
    Resolve a setting the way the running app actually sees it.

    .NET loads appsettings.json, then user-secrets, then environment variables last. So an
    env var set from .env by run-api-local.ps1 BEATS user-secrets. Checking user-secrets
    first therefore reads the wrong secret whenever .env defines the value - which is
    exactly what made OTP brute force silently find nothing.
    """
    return os.environ.get(env_key) or dotenv.get(env_key) or secrets.get(secret_key, "")


def recover_otp(identifier: str, purpose: str, secret: str, length: int = 6) -> str | None:
    """
    Brute-force the 6-digit code against the stored HMAC.

    OtpService.HashCode is HMACSHA256(secret, "{purpose}:{identifier}:{code}") in Base64.
    Knowing the secret and that the code is numeric makes this a 10^6 search, which is
    fast, and it only ever touches a local dev database you own.
    """
    rows = sql(
        "SELECT TOP 1 CodeHash FROM OtpVerifications "
        f"WHERE Identifier = '{identifier}' AND Purpose = '{purpose}' AND IsConsumed = 0 "
        "ORDER BY Expiry DESC"
    )
    if not rows:
        return None
    target = rows[0][0]
    key = secret.encode("utf-8")
    prefix = f"{purpose}:{identifier}:".encode("utf-8")
    for n in range(10 ** length):
        code = f"{n:0{length}d}".encode("ascii")
        digest = base64.b64encode(hmac.new(key, prefix + code, hashlib.sha256).digest()).decode()
        if hmac.compare_digest(digest, target):
            return code.decode()
    return None


# ------------------------------------------------------------------- resolver
def _retry_after(value):
    """Seconds to wait before retrying, from the API's Retry-After header.

    Retry-After is either delta-seconds or an HTTP date. Only delta-seconds is honoured;
    an unparsable value yields None so the caller falls back to its own minimum.
    """
    if not value:
        return None
    try:
        return float(value.strip())
    except ValueError:
        return None


def resolve_path(path: str, var: dict) -> str:
    """Substitute {{var}}, URL-encoding any value that lands in the path."""
    def sub(m):
        key = m.group(1).strip()
        val = var.get(key, "")
        return urllib.parse.quote(str(val), safe="")
    # a literal query string may legitimately hold an unencoded value
    if "?" in path:
        head, _, tail = path.partition("?")
        return TEMPLATE.sub(sub, head) + "?" + TEMPLATE.sub(lambda m: str(var.get(m.group(1).strip(), "")), tail)
    return TEMPLATE.sub(sub, path)


def resolve_obj(obj, var: dict):
    if isinstance(obj, str):
        # A value that is exactly one placeholder adopts the variable's natural type, so a
        # body of "{{someIdList}}" sends a real JSON array rather than a quoted string.
        whole = TEMPLATE.fullmatch(obj.strip())
        if whole:
            val = var.get(whole.group(1).strip())
            if isinstance(val, str) and val[:1] in "[{":
                try:
                    return json.loads(val)
                except ValueError:
                    return val
            return val
        return TEMPLATE.sub(lambda m: str(var.get(m.group(1).strip(), "")), obj)
    if isinstance(obj, dict):
        # Keys are substituted too: reorder payloads are keyed by entity id, e.g.
        # {"sectionOrders": {"{{sectionId}}": 2}}. Leaving the placeholder in the key sent
        # the API a literal "{{sectionId}}" property and it rejected the whole body.
        return {
            TEMPLATE.sub(lambda m: str(var.get(m.group(1).strip(), "")), k) if isinstance(k, str) else k:
                resolve_obj(v, var)
            for k, v in obj.items()
        }
    if isinstance(obj, list):
        return [resolve_obj(v, var) for v in obj]
    return obj


TOKEN = re.compile(r'\[\s*"([^"]+)"\s*\]|\[\s*(\w+)\s*=\s*([^\]]+?)\s*\]|\[\s*(\d+)\s*\]|\[\s*(\*)\s*\]|([A-Za-z_][A-Za-z0-9_]*)')


def pick(obj, path: str):
    """Tiny JSONPath subset: $.a.b, $.a[0].b, $.a["x"].b, $.a[k=V].b, $.a[*].b.

    Supports [key=Value] so a step can target one named row instead of whatever happens to
    be first, and [*] so a step can capture a whole list of ids for a read-modify-write.
    """
    if not path.startswith("$"):
        return None
    rest = path[1:]
    for m in TOKEN.finditer(rest):
        quoted, key, val, idx, star, name = m.groups()
        if star:
            tail = rest[m.end():]
            if not isinstance(obj, list):
                return None
            return [pick(v, "$" + tail) for v in obj]
        if quoted or name:
            cur = obj.get(quoted or name) if isinstance(obj, dict) else None
        elif key:
            cur = next((v for v in obj if isinstance(v, dict) and str(v.get(key)) == val), None) \
                if isinstance(obj, list) else None
        else:
            cur = obj[int(idx)] if isinstance(obj, list) and int(idx) < len(obj) else None
        if cur is None:
            return None
        obj = cur
    return obj


def find_id(obj, prop: str = "id"):
    """Find the first object carrying `prop`, searching nested lists/dicts."""
    if isinstance(obj, dict):
        if prop in obj and isinstance(obj[prop], str):
            return obj[prop]
        for v in obj.values():
            got = find_id(v, prop)
            if got:
                return got
    elif isinstance(obj, list):
        for v in obj:
            got = find_id(v, prop)
            if got:
                return got
    return None


# --------------------------------------------------------------------- runner
class Runner:
    def __init__(self, base: str, verbose: bool = False):
        self.base = base.rstrip("/")
        self.ctx = ssl.create_default_context()
        self.ctx.check_hostname = False
        self.ctx.verify_mode = ssl.CERT_NONE
        self.plan = json.loads(PLAN_PATH.read_text(encoding="utf-8"))
        self.var = {}
        for name, meta in self.plan["variables"].items():
            self.var[name] = str(meta.get("value") or "")
        self.results = []
        self.broken_captures = []
        self.retry_after = None
        self.missing_files = []
        self.verbose = verbose

    # ---- http
    def with_query(self, path: str, query) -> str:
        """Append a step's query object to the path.

        The plan keeps query parameters in a "query" object rather than inline in the path,
        so they have to be merged here. Values still containing a placeholder are dropped:
        sending the literal "{{id}}" would produce a confusing 400 that looks like an API
        defect. An empty value is dropped for the same reason.
        """
        if not query:
            return path
        parts = []
        for k, v in query.items():
            v = resolve_obj(v, self.var)
            v = str(v) if v is not None else ""
            if v == "" or TEMPLATE.search(v):
                continue
            parts.append(f"{urllib.parse.quote(str(k), safe='')}={urllib.parse.quote(v, safe='')}")
        if not parts:
            return path
        sep = "&" if "?" in path else "?"
        return path + sep + "&".join(parts)

    def request(self, method, path, headers, body):
        req = urllib.request.Request(self.base + path, data=body, method=method)
        for k, v in headers.items():
            if v is not None:
                req.add_header(k, v)
        try:
            with urllib.request.urlopen(req, context=self.ctx, timeout=45) as r:
                raw = r.read()
                self.retry_after = _retry_after(r.headers.get("Retry-After"))
                return r.status, self._decode(raw, r.headers.get("Content-Type", ""))
        except urllib.error.HTTPError as e:
            self.retry_after = _retry_after(e.headers.get("Retry-After"))
            return e.code, self._decode(e.read(), e.headers.get("Content-Type", ""))
        except Exception as e:  # connection level failure
            self.retry_after = None
            return 0, {"_error": str(e)}

    @staticmethod
    def _decode(raw, ctype):
        if not raw:
            return {}
        try:
            return json.loads(raw)
        except Exception:
            return {"_text": raw[:400].decode("utf-8", "replace")}

    # ---- multipart
    def multipart(self, fields, files):
        boundary = "----plan" + uuid.uuid4().hex
        chunks = []
        for k, v in fields.items():
            chunks.append(
                f'--{boundary}\r\nContent-Disposition: form-data; name="{k}"\r\n\r\n{v}\r\n'.encode()
            )
        for k, (name, data) in files.items():
            head = (f'--{boundary}\r\nContent-Disposition: form-data; name="{k}"; '
                    f'filename="{name}"\r\nContent-Type: application/octet-stream\r\n\r\n')
            chunks.append(head.encode() + data + b"\r\n")
        chunks.append(f"--{boundary}--\r\n".encode())
        return b"".join(chunks), {"Content-Type": f"multipart/form-data; boundary={boundary}"}

    # ---- execution
    def run_step(self, step):
        order, sid = step["order"], step["id"]
        method = step["method"].upper()
        path = resolve_path(step["path"], self.var)
        path = self.with_query(path, step.get("query"))

        # A step the plan marks as skipped is never sent. It still appears in the report so
        # the coverage stays visible, but it cannot pass or fail.
        if step.get("skip"):
            return {
                "order": order, "id": sid, "name": step["name"], "phase": step["phase"],
                "method": method, "path": path, "status": 0, "expected": step.get("expectStatus") or [],
                "ok": None, "data": {}, "note": step["skip"], "skipped": True,
            }

        headers = {"Accept": "application/json"}
        token = self.var.get(TOKEN_VAR.get(step["auth"], ""), "")
        if step["auth"] in TOKEN_VAR:
            headers["Authorization"] = f"Bearer {token}"

        body = None
        form = step.get("form") or {}
        opt = step.get("optionalForm") or {}

        if form or opt:
            resolved = resolve_obj({**form, **opt}, self.var)
            files = {}
            plain = {}
            for k, v in resolved.items():
                if isinstance(v, str) and PATHY.match(v):
                    p = pathlib.Path(v)
                    if p.exists():
                        files[k] = (p.name, p.read_bytes())
                    else:
                        # Previously a missing file was replaced with a one-byte stand-in,
                        # so the API happily accepted a "thumbnail" that was not an image and
                        # the step looked green. Report it instead of faking the upload.
                        self.missing_files.append((step["id"], k, v))
                        files[k] = (p.name, b"")
                else:
                    plain[k] = v
            body, hdrs = self.multipart(plain, files)
            headers.update(hdrs)
        elif step.get("body") is not None:
            body = json.dumps(resolve_obj(step["body"], self.var)).encode()
            headers["Content-Type"] = "application/json"

        for k, v in (step.get("headers") or {}).items():
            headers[k] = str(resolve_obj(v, self.var))

        status, data = self.request(method, path, headers, body)

        # Rate limits are a property of the plan's own pacing, not a defect. Back off and
        # retry; if the window is still closed after the retries, report SKIPPED rather than
        # FAIL so the summary does not blame the API for a limit it enforces correctly.
        skipped = False
        for attempt in range(4):
            if status != 429:
                break
            if attempt == 3:
                skipped = True
                break
            # The OTP policies use a TEN minute window, so a fixed short sleep just burns
            # retries and the step is reported SKIPPED. The API publishes Retry-After;
            # honour it (plus a small margin) so a re-run inside the window recovers.
            time.sleep(min(max(self.retry_after or 0, 2) + 2, 620))
            status, data = self.request(method, path, headers, body)

        # Some writes land asynchronously - the chat notification, for example, is created by
        # a queued event handler, so the row exists a second or two after the message is sent.
        # A read that immediately follows would see an empty list and capture nothing. Such a
        # step declares pollFor and is re-issued until the named captures resolve. Only put it
        # on idempotent steps (reads): re-issuing a POST would repeat its side effects.
        poll = step.get("pollFor")
        if poll and not skipped and 200 <= status < 300:
            wanted = poll["var"] if isinstance(poll, dict) else poll
            limit = (poll.get("seconds", 60) if isinstance(poll, dict) else 60)
            interval = (poll.get("interval", 3) if isinstance(poll, dict) else 3)
            deadline = time.time() + limit
            while wanted not in self.resolve_captures(step, data) and time.time() < deadline:
                time.sleep(interval)
                status, data = self.request(method, path, headers, body)

        expected = step.get("expectStatus") or [200]
        if skipped:
            ok = None
        else:
            ok = status in expected
        return {
            "order": order, "id": sid, "name": step["name"], "phase": step["phase"],
            "method": method, "path": path, "status": status, "expected": expected,
            "ok": ok, "data": data, "note": step.get("notes"),
            "skipped": skipped,
        }

    def resolve_captures(self, step, data):
        """Read every declared capture out of one response, without storing it."""
        values = {}
        for var, path in (step.get("capture") or {}).items():
            if path.startswith("$"):
                val = pick(data, path)
            elif path == "id of any unread item":
                val = find_id(data, "id")
            else:
                # "id of the X just written" -> pull it out of this very response
                val = find_id(data)
            if val is not None and str(val) != "":
                # A wildcard capture yields a list; keep it as JSON so it can be sent
                # straight back as a request body (read-modify-write round trips).
                values[var] = json.dumps(val) if isinstance(val, list) else str(val)
        return values

    def apply_captures(self, step, data):
        """Store captured variables, and remember any that the response did not contain.

        A capture that silently resolves to nothing is the most damaging kind of plan bug:
        the step itself passes, and every later step that uses the variable then sends a
        literal {{courseId}} and 404s. Recording the miss makes that visible.
        """
        values = self.resolve_captures(step, data)
        for var, path in (step.get("capture") or {}).items():
            val = values.get(var)
            if val is not None:
                self.var[var] = val
            else:
                self.broken_captures.append({
                    "id": step["id"], "var": var, "path": path,
                    "status": step.get("_last_status"),
                })

# ----------------------------------------------------------------------- main
def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--base-url", default="https://localhost:7271")
    ap.add_argument("--only", nargs="*", default=None)
    ap.add_argument("--skip", nargs="*", default=[])
    ap.add_argument("--out", default=None, help="write a JSON report here")
    ap.add_argument("--fixed-emails", action="store_true",
                    help="use the emails hard-coded in the plan instead of run-scoped ones")
    args = ap.parse_args()

    r = Runner(args.base_url)
    secrets = user_secrets()
    dotenv = load_dotenv()

    # Upload fixtures live in the repo so the plan can reference a real file instead of a
    # hand-edited machine path. Expand to an absolute path; on Windows a relative path in
    # a multipart filename is rejected or silently posted empty.
    fixtures = ROOT / "docs" / "test-files"
    r.var["testFilesDir"] = str(fixtures).replace("/", "\\")
    if not fixtures.is_dir():
        print(f"WARNING: upload fixtures missing at {fixtures}")
        print("         run: python docs/make_test_files.py")
    else:
        print(f"upload fixtures: {fixtures}")

    # --- operator-supplied values
    admin_email = app_setting(dotenv, secrets, "SEED_SUPERADMIN_EMAIL", "Seed:SuperAdmin:Email")
    admin_password = app_setting(dotenv, secrets, "SEED_SUPERADMIN_PASSWORD", "Seed:SuperAdmin:Password")
    if admin_email:
        r.var["adminEmail"] = admin_email
    if admin_password:
        r.var["adminPassword"] = admin_password

    otp_secret = app_setting(dotenv, secrets, "OTP_SECRET", "OtpSettings:HashingSecret")
    if not otp_secret:
        print("warn: no OTP hashing secret found in .env, environment or user-secrets; "
              "OTP steps cannot be automated.")
    else:
        print(f"OTP secret resolved from: "
              f"{'environment' if os.environ.get('OTP_SECRET') else '.env' if dotenv.get('OTP_SECRET') else 'user-secrets'}")

    # The invitation token is captured from S070's own response now, so there is no
    # database lookup here. Reading "ORDER BY CreatedAt DESC" was both unnecessary and
    # wrong: SendStaffInvitationAsync expires every other pending invitation on each send,
    # so the newest row is not reliably the one this run is about to validate.

    steps = r.plan["steps"]
    if args.only:
        steps = [s for s in steps if s["id"] in args.only]
    steps = [s for s in steps if s["id"] not in args.skip]

    # The plan hard-codes student1@test.com etc, so a second run collides on the unique
    # email index (409). Give each run its own addresses rather than editing the plan,
    # which stays human-facing and readable. Only used when the caller does not pin them.
    if not args.fixed_emails:
        stamp = time.strftime("%m%d") + str(int(time.time()) % 100000)
        r.var["student1Email"] = f"student1-{stamp}@test.com"
        r.var["student2Email"] = f"student2-{stamp}@test.com"
        print(f"run-scoped emails: {r.var['student1Email']} / {r.var['student2Email']}")

        # Categories and promo codes are unique too, so the same rerun problem applies:
        # S090 collides with Category.DuplicateSlug and S183 with the promo code. Suffix
        # them so a second run reaches the assertions instead of dying on a 409. The plan
        # keeps readable defaults; only the live run is suffixed.
        r.var["categorySlug"] = f"programming-{stamp}"
        r.var["throwawayCategorySlug"] = f"disposable-{stamp}"
        r.var["promoCodeSeed"] = f"WELCOME10-{stamp}"
        r.var["promoCode"] = r.var["promoCodeSeed"]
        # S081 sets a phone number that has a unique index, so it needs a per-run value too.
        r.var["runStamp"] = stamp[-7:]
        print(f"run-scoped slugs: {r.var['categorySlug']} / {r.var['promoCodeSeed']}")

    # var name -> (email variable, OtpPurpose). Purpose is part of the HMAC input, so a
    # password-reset code can never be recovered with the email-verification purpose.
    otp_done = set()
    OTP_SPECS = {
        "student1Otp": ("student1Email", "EmailVerification"),
        "student2Otp": ("student2Email", "EmailVerification"),
        "student1ResetOtp": ("student1Email", "PasswordReset"),
    }
    results = []
    started = time.time()

    print(f"Running {len(steps)} steps against {args.base_url}\n")
    print(f"{'#':>4}  {'step':<7} {'code':<5} {'ok':<5} endpoint")
    print("-" * 96)

    for step in steps:
        # Refresh an OTP right before the step that needs it. Recomputed every time rather
        # than once, because a resend or a second request rotates the code and a cached
        # value would silently fail verification.
        for var in (step.get("manualInput") or []):
            spec = OTP_SPECS.get(var)
            if not spec:
                continue
            email_var, purpose = spec
            ident = r.var.get(email_var, "")
            if not ident or not otp_secret:
                continue
            code = recover_otp(ident, purpose, otp_secret)
            if code:
                r.var[var] = code
                if (var, code) not in otp_done:
                    otp_done.add((var, code))
                    print(f"     ..recovered {var} for {ident} ({purpose})")

        res = r.run_step(step)
        step["_last_status"] = res["status"]
        # A skipped step produces no response, so capturing from it would only manufacture
        # phantom "capture found nothing" reports for variables nothing ever returned.
        if not step.get("skip"):
            r.apply_captures(step, res["data"])
        results.append(res)
        if res["skipped"]:
            flag = "SKIP"
        else:
            flag = "PASS" if res["ok"] else "FAIL"
        extra = ""
        if step.get("skip"):
            extra = "   excluded from the plan: " + step["skip"].split(".")[0]
        elif res["skipped"]:
            extra = "   still 429 after 3 backoffs - rate limit window, not an API defect"
        elif not res["ok"]:
            extra = f"   expected {res['expected']}"
        print(f"{res['order']:>4}  {res['id']:<7} {res['status']:<5} {flag:<5} "
              f"{res['method']} {res['path'][:58]}{extra}")

    passed = sum(1 for x in results if x["ok"])
    failed = sum(1 for x in results if x["ok"] is False)
    skipped = sum(1 for x in results if x["skipped"])
    print("-" * 96)
    print(f"{passed} passed, {failed} failed, {skipped} skipped "
          f"({len(results)} total) in {time.time()-started:.0f}s")

    if r.broken_captures:
        print()
        print(f"=== captures that found nothing in the response ({len(r.broken_captures)}) ===")
        print("    Each of these leaves its variable empty, so every later step that")
        print("    uses it sends a literal {{name}} instead of an id.")
        for b in r.broken_captures:
            print(f"  {b['id']:<7} {b['var']:<24} <- '{b['path']}'  (step HTTP {b['status']})")

    if r.missing_files:
        print()
        print(f"=== upload fixtures that do not exist ({len(r.missing_files)}) ===")
        print("    These were posted as empty files. Run: python docs/make_test_files.py")
        for sid, field, path in r.missing_files:
            print(f"  {sid:<7} {field:<18} {path}")

    if args.out:
        pathlib.Path(args.out).write_text(json.dumps(results, indent=2, default=str), encoding="utf-8")
        print(f"report -> {args.out}")

    return 0 if passed == len(results) else 1


if __name__ == "__main__":
    sys.exit(main())
