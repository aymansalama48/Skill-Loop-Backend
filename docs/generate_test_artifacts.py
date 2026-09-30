"""
Generates executable test artifacts from docs/scalar-test-plan.json:

  docs/api-tests.http                      -> VS Code REST Client, one click per step
  docs/skill-loop.postman_collection.json   -> Postman v2.1, "Run Collection" executes everything

Run:  python docs/generate_test_artifacts.py
"""
import json
import pathlib
import re
import uuid

ROOT = pathlib.Path(__file__).resolve().parent.parent
PLAN = ROOT / "docs" / "scalar-test-plan.json"
HTTP_OUT = ROOT / "docs" / "api-tests.http"
POSTMAN_OUT = ROOT / "docs" / "skill-loop.postman_collection.json"

AUTH_HEADER = {
    "adminToken": "adminToken",
    "userToken": "userToken",
    "user2Token": "user2Token",
    "instructorToken": "instructorToken",
    "devAdminToken": "devAdminToken",
}

# vars the operator must fill in by hand; keep them out of the auto-capture list
MANUAL_VARS = {
    "adminEmail", "adminPassword", "student1Email", "student1Otp",
    "student1Password", "student2Email", "student2Otp", "invitationToken",
}

GUID_RE = re.compile(r"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$")
# a form value that looks like "dir/name.ext" (hyphens allowed in the name) is a file upload
FILE_RE = re.compile(r"^[\\/]?[\w\-./\\ ]*[\w\-]\.\w{2,4}$")
PATHY_RE = re.compile(r"^[A-Za-z]:[\\/]")


def guid_placeholder(value: str) -> bool:
    return isinstance(value, str) and bool(GUID_RE.match(value.strip()))


def jsonpath_to_js(path: str) -> str:
    """$.data.accessToken -> pm.response.json().data.accessToken"""
    parts = [p for p in path.split(".") if p and p != "$"]
    if not parts:
        return "pm.response.text()"
    expr = "pm.response.json()"
    for p in parts:
        expr += f".{p}" if p.isidentifier() else f"[{json.dumps(p)}]"
    return expr


def clean(value):
    """Replace machine-specific absolute paths with a placeholder you must edit."""
    if isinstance(value, str):
        if re.match(r"^[A-Za-z]:[\\/]", value):
            return "C:/path/to/" + value.replace("\\", "/").rsplit("/", 1)[-1]
        return value
    if isinstance(value, dict):
        return {k: clean(v) for k, v in value.items()}
    if isinstance(value, list):
        return [clean(v) for v in value]
    return value


def url_encode_query_value(value) -> str:
    """A bare '#' or space in a .http request line can truncate the URL, so encode them."""
    if isinstance(value, str):
        return value.replace("#", "%23").replace(" ", "%20")
    return value


def full_target(step) -> str:
    """Path with any query parameters inlined, values substituted as {{var}} for the .http file."""
    target = step["path"]
    q = step.get("query") or {}
    if not q:
        return target
    sep = "&" if "?" in target else "?"
    return target + sep + "&".join(
        f"{k}={url_encode_query_value(v)}" for k, v in q.items()
    )


def humanize(step) -> str:
    notes = step.get("notes")
    return f"{step['id']} - {step['name']}" + (f"\n;  NOTE: {notes}" if notes else "")


# ----------------------------------------------------------------- .http file
def build_http(plan) -> str:
    out = []
    scalar = plan["scalar"]

    out.append("// " + "=" * 96)
    out.append(f"// {plan['name']}")
    out.append("// GENERATED FILE - do not edit by hand.")
    out.append("// Source of truth: docs/scalar-test-plan.json   Regenerate: python docs/generate_test_artifacts.py")
    out.append("//")
    out.append("// HOW TO RUN - no Docker and no Redis required")
    out.append("//")
    out.append("//   1. Start the API. This one script resolves the security secrets, points")
    out.append("//      at the SQL Server instance that is actually installed, and switches")
    out.append("//      Redis off in favour of the in-memory cache:")
    out.append("//          powershell -ExecutionPolicy Bypass -File docs\\run-api-local.ps1")
    out.append("//      It creates the database, applies migrations and seeds the admin")
    out.append("//      account on first start, so there is nothing else to set up.")
    out.append("//      Wait for 'Now listening on: https://localhost:7271'.")
    out.append("//")
    out.append("//   2. Install the VS Code extension 'REST Client' (humao.rest-client),")
    out.append("//      open this file and click 'Send Request' above any step.")
    out.append("//")
    out.append("//   3. Work down the file in order. Each step stores what it returns, so the")
    out.append("//      ids further down fill themselves in and you never copy one by hand.")
    out.append("//")
    out.append("//   Seeded login:  admin@skillloop.com / Password@123")
    out.append("//   Every other account used below is created by the plan itself.")
    out.append("//")
    out.append("//   To run the whole thing automatically instead, import")
    out.append("//   docs/skill-loop.postman_collection.json into Postman and use the")
    out.append("//   collection runner.")
    out.append("//")
    out.append(f"// Scalar UI (for browsing, not for running): {scalar['ui']}")
    out.append("// " + "=" * 96)
    out.append("")
    out.append("@baseUrl = " + scalar["ui"].rsplit("/scalar", 1)[0])
    out.append("")
    out.append("// ---- tokens: filled in automatically by the login steps, leave empty ----")
    for name in ("adminToken", "userToken", "user2Token", "instructorToken", "devAdminToken"):
        out.append(f"@{name} =")
    out.append("")
    out.append("// ---- ids: filled in automatically by the create steps, leave empty ----")
    for name in sorted(set(plan["variables"]) - MANUAL_VARS
                       - {"adminToken", "userToken", "user2Token", "instructorToken", "devAdminToken"}):
        out.append(f"@{name} =")
    out.append("")
    out.append("// ---- values you MUST fill in yourself (they arrive by email, not from the API) ----")
    for name in sorted(MANUAL_VARS):
        out.append(f"@{name} =")
    out.append("")

    for step in plan["steps"]:
        out.append("#" * 96)
        out.append(f"### [{step['order']:>3}] {step['id']}  {step['name']}")
        out.append(f"### phase: {step['phase']}   auth: {step['auth']}   expect: {step['expectStatus']}")
        if step.get("notes"):
            out.append(f"### note: {step['notes']}")
        if step.get("manualInput"):
            out.append(f"### MANUAL INPUT REQUIRED: fill {', '.join(step['manualInput'])} before running")
        if step.get("skipIfStepCompleted"):
            out.append(f"### SKIP if you already ran {step['skipIfStepCompleted']}")
        out.append("")
        out.append(f"{step['method']} {full_target(step)}")
        out.append("Accept: application/json")

        if step["auth"] in AUTH_HEADER:
            out.append(f"Authorization: Bearer {{{{{AUTH_HEADER[step['auth']]}}}}}")

        if step.get("headers"):
            for k, v in step["headers"].items():
                out.append(f"{k}: {v}")

        body = step.get("body")
        form = step.get("form")
        optional_form = step.get("optionalForm")

        if form or optional_form:
            merged = dict(form or {})
            merged.update(optional_form or {})
            out.append("Content-Type: multipart/form-data")
            out.append("")
            out.append("// Attach a real file to the fields noted below in the REST Client panel.")
            for k, v in merged.items():
                out.append(f"// file field: {k}")
                out.append(f"{k} = {clean(v)}")
        elif body is not None:
            out.append("Content-Type: application/json")
            out.append("")
            out.append(json.dumps(clean(body), indent=2, ensure_ascii=False))
        else:
            out.append("")

        out.append("")

        script = []
        for var, path in (step.get("capture") or {}).items():
            if var in MANUAL_VARS or "first item id" in path or "response" in path.lower():
                continue
            if path.startswith("$"):
                script.append(
                    f'> {{% client.global.set("{var}", {jsonpath_to_js(path)}); %}}'
                )
            else:
                script.append(
                    f'> {{% client.global.set("{var}", {jsonpath_to_js("$")}["{path}"]); %}}'
                )
        if script:
            out.extend(script)
            out.append("")

    return "\n".join(out) + "\n"


# ---------------------------------------------------------- postman collection
def build_postman(plan) -> dict:
    # Every plan variable gets a collection variable, so nothing ever resolves to a
    # literal "{{courseId}}" in the request URL before the creating step has run.
    coll_vars = [{"key": "baseUrl", "value": plan["scalar"]["ui"].rsplit("/scalar", 1)[0], "type": "string"}]
    for name, meta in plan["variables"].items():
        coll_vars.append({"key": name, "value": str(meta.get("value") or ""), "type": "string"})

    phase_names = {p["id"]: p["name"] for p in plan["phases"]}
    by_phase: dict[str, list] = {}
    for step in plan["steps"]:
        by_phase.setdefault(step["phase"], []).append(step)

    def url_for(step):
        path = step["path"]
        q = step.get("query") or {}
        raw = "{{baseUrl}}" + path
        if not q:
            return {"raw": raw, "host": ["{{baseUrl}}"], "path": path.strip("/").split("/")}
        parts = []
        for k, v in q.items():
            parts.append(f"{k}={{{k}}}")
            coll_vars.append({"key": k, "value": str(v), "type": "string"})
        sep = "&" if "?" in raw else "?"
        return {"raw": f"{raw}{sep}{'&'.join(parts)}", "host": ["{{baseUrl}}"],
                "path": path.strip("/").split("/"), "query": [
                    {"key": k, "value": str(v)} for k, v in q.items()]}

    def request_for(step):
        req = {
            "method": step["method"],
            "header": [
                {"key": "Accept", "value": "application/json"},
            ],
            "url": url_for(step),
            "description": step.get("notes") or step["name"],
        }

        if step["auth"] in AUTH_HEADER:
            req["header"].append({"key": "Authorization", "value": "Bearer {{" + AUTH_HEADER[step["auth"]] + "}}"})

        for k, v in (step.get("headers") or {}).items():
            req["header"].append({"key": k, "value": v, "type": "text"})

        form = step.get("form")
        optional_form = step.get("optionalForm")
        if form or optional_form:
            merged = dict(form or {})
            merged.update(optional_form or {})
            body = []
            for k, v in merged.items():
                if isinstance(v, str) and PATHY_RE.match(v):
                    body.append({"key": k, "type": "file", "src": clean(v),
                                 "description": "EDIT THIS PATH before running"})
                else:
                    body.append({"key": k, "value": clean(v), "type": "text"})
            req["body"] = {"mode": "formdata", "formdata": body}

        elif step.get("body") is not None:
            raw = json.dumps(clean(step["body"]), indent=2, ensure_ascii=False)
            req["header"].append({"key": "Content-Type", "value": "application/json"})
            req["body"] = {"mode": "raw", "raw": raw, "options": {"raw": {"language": "json"}}}

        return req

    def events_for(step):
        lines = []
        codes = ", ".join(str(c) for c in step["expectStatus"])
        lines.append(f'pm.test("[{step["order"]}] {step["id"]} status", function () {{')
        lines.append(f'    pm.expect(pm.response.code, "expected one of [{codes}]").to.be.oneOf([{codes}]);')
        lines.append("});")

        for var, path in (step.get("capture") or {}).items():
            if var in MANUAL_VARS or "first item id" in path:
                continue
            if path.startswith("$"):
                expr = jsonpath_to_js(path)
            else:
                expr = f'(pm.response.json()["{path}"])'
            lines.append("")
            lines.append(f"// capture {var}")
            lines.append("var _v = (function(){ try { return " + expr + "; } catch (e) { return null; } })();")
            lines.append(f'if (_v !== null && _v !== undefined && _v !== "") {{ pm.collectionVariables.set("{var}", String(_v)); }}')

        return [{"listen": "test", "script": {"type": "text/javascript", "exec": lines}}]

    items = []
    for phase_id, steps in by_phase.items():
        requests = []
        for step in steps:
            requests.append({
                "name": f"{step['order']:>3} | {step['id']} | {step['name']}",
                "event": events_for(step),
                "request": request_for(step),
                "response": [],
            })
        items.append({
            "name": f"{phase_id} - {phase_names.get(phase_id, phase_id)}",
            "item": requests,
        })

    return {
        "info": {
            "name": "Skill Loop Backend - Full API Test Suite",
            "_postman_id": str(uuid.uuid5(uuid.NAMESPACE_DNS, "skill-loop-api-tests")),
            "description": (
                "GENERATED from docs/scalar-test-plan.json - do not edit by hand.\n"
                "Regenerate: python docs/generate_test_artifacts.py\n\n"
                "HOW TO RUN\n"
                "  1. Import this file into Postman (Import > Link/Disk).\n"
                "  2. Start the API: dotnet run --project src/Skill-Loop.Api\n"
                "  3. Set collection variable student1Otp / student2Otp / invitationToken manually.\n"
                "  4. Edit any file paths marked 'EDIT THIS PATH before running'.\n"
                "  5. Collection > Run (Ctrl+Alt+R). It executes all steps in order and each\n"
                "     response feeds the next request automatically.\n\n"
                "Docker is NOT required. Scalar is served in-process at /scalar/v1.\n"
            ),
            "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json",
        },
        "variable": coll_vars,
        "item": items,
        "auth": {"type": "bearer", "bearer": [{"key": "token", "value": "{{adminToken}}", "type": "string"}]},
    }


def main():
    plan = json.loads(PLAN.read_text(encoding="utf-8"))

    HTTP_OUT.write_text(build_http(plan), encoding="utf-8")
    POSTMAN_OUT.write_text(json.dumps(build_postman(plan), indent=2, ensure_ascii=False) + "\n", encoding="utf-8")

    print(f"steps           : {len(plan['steps'])}")
    print(f"wrote {HTTP_OUT.relative_to(ROOT)}")
    print(f"wrote {POSTMAN_OUT.relative_to(ROOT)}")


if __name__ == "__main__":
    main()
