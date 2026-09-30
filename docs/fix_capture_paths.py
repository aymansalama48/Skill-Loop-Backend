"""
Find plan steps whose `capture` path is wrong.

Commands declared as `ICommand<Guid>` (and other scalars) return `{"data": "<guid>"}`,
not `{"data": {"id": ...}}`. A `$.data.id` capture therefore resolves to undefined, the
variable is never set, and every later step sends a literal {{courseId}}.

The OpenAPI has no operationIds and no response schemas, so this maps it from the source:
1. find command records whose return type is a scalar,
2. find the controller action that sends that command,
3. match that route+method to the plan and rewrite the capture.
"""
import json
import pathlib
import re
import sys

ROOT = pathlib.Path(__file__).resolve().parent.parent
APP = ROOT / "src" / "Skill-Loop.Application"
API = ROOT / "src" / "Skill-Loop.Api" / "Controllers"
PLAN = ROOT / "docs" / "scalar-test-plan.json"

TEMPLATE = re.compile(r"\{\{[^}]*\}\}")
PARAM = re.compile(r"\{[^}]*\}")
# :name route constraints must be dropped before comparing with the plan
CONSTRAINT = re.compile(r":[A-Za-z]+(?=[}/])")


def norm(p):
    p = TEMPLATE.sub("{}", p)
    p = PARAM.sub("{}", p)
    p = CONSTRAINT.sub("", p)
    p = p.lower().split("?", 1)[0]
    # The plan writes "/api/v1/Courses"; a controller route builds up to "api/v1/courses"
    # because the leading slash gets consumed by the join. Without this the two never match
    # and every capture silently stays wrong.
    return p.strip("/")


scalar_cmds = {}
# Both shapes return a bare Guid and therefore a bare {"data": "<guid>"}:
#   ICommand<Guid>                 (most commands)
#   IRequest<Result<Guid>>         (a few, e.g. CreatePromoCodeCommand)
PATTERNS = (
    r"record\s+(\w+Command)\s*\([^)]*\)\s*:\s*ICommand<([^>]+)>",
    r"record\s+(\w+Command)\s*\([^)]*\)\s*:\s*I(?:Command|Request)<Result<([^>]+)>>",
)
for f in APP.rglob("*Command.cs"):
    text = f.read_text(encoding="utf-8", errors="replace")
    for pat in PATTERNS:
        for m in re.finditer(pat, text, re.S):
            name, ret = m.group(1), m.group(2).strip()
            if ret in {"Guid", "bool", "string", "int"}:
                scalar_cmds[name] = ret

print(f"scalar-returning commands: {len(scalar_cmds)}")

# Which (METHOD, path) invokes each scalar command? Controllers build the command inside
# the method body (`var command = new XCommand(...); Mediator.Send(command)`), so the file
# is split on [Http...] boundaries and each action's own chunk is scanned.
scalar_routes = {}
for f in API.rglob("*.cs"):
    src = f.read_text(encoding="utf-8", errors="replace")
    # ASP.NET substitutes [controller] with the class name minus the "Controller" suffix.
    cls = re.search(r"class\s+(\w+Controller)\b", src)
    controller_token = (cls.group(1)[: -len("Controller")] if cls else "").lower()
    prefix = ""
    pm = re.search(r'\[Route\(\s*"([^"]+)"', src)
    if pm:
        prefix = pm.group(1).replace("[controller]", controller_token)

    parts = list(re.finditer(
        r'\[Http(?P<verb>Get|Post|Put|Delete|Patch)(?P<tail>[^\]]*)\]', src))
    for i, am in enumerate(parts):
        end = parts[i + 1].start() if i + 1 < len(parts) else len(src)
        body = src[am.start():end]

        sent = re.findall(r"new\s+(\w+Command)\s*\(", body)
        if not sent:
            continue

        tail = am.group("tail")
        route = ""
        if tm := re.search(r'Route\s*=\s*"([^"]*)"', tail):
            route = tm.group(1)
        else:
            m2 = re.match(r'\s*\(\s*"([^"]*)"', tail)
            route = m2.group(1) if m2 else ""
        # [Route("api/v1/[controller]")] has to be expanded to the controller name.
        if "[controller]" in route:
            route = route.replace("[controller]", controller_token)
        full = (prefix.rstrip("/") + "/" + route.lstrip("/")).rstrip("/") or "/"
        for c in sent:
            if c in scalar_cmds:
                scalar_routes.setdefault((am.group("verb").upper(), norm(full)), set()).add(
                    (c, scalar_cmds[c])
                )

print(f"routes that invoke a scalar command: {len(scalar_routes)}\n")

plan = json.loads(PLAN.read_text(encoding="utf-8"))
fixed, listed = [], []
for s in plan["steps"]:
    key = (s["method"].upper(), norm(s["path"]))
    if key not in scalar_routes:
        continue
    cmds = scalar_routes[key]
    types = {t for _, t in cmds}
    ids = sorted(c for c, _ in cmds)
    # Only rewrite when every command behind this route returns a bare Guid. A route shared
    # with a non-scalar command (e.g. the shared POST /api/v1/[controller] bucket) is left
    # alone, because a capture that is right for one command is wrong for the other.
    if not types <= {"Guid"}:
        continue
    for var, p in list((s.get("capture") or {}).items()):
        if p.startswith("$") and p != "$.data":
            tail = p.split(".")[2:] if p.startswith("$.data.") else []
            if tail == ["id"]:
                listed.append((s["order"], s["id"], s["path"], var, p, ids))
                fixed.append((s["id"], var, p))

print("=== routes that invoke a scalar (Guid) command ===")
for (verb, path), cmds in sorted(scalar_routes.items()):
    print(f"  {verb:<6} {path:<58} {sorted(c for c, _ in cmds)}")

print()
print("=== steps whose capture $.data.id must become $.data ===")
for order, sid, path, var, p, ids in listed:
    print(f"  [{order:>3}] {sid:<7} {path:<58} {var} <- {p}   ({', '.join(ids)})")
print(f"\n{len(fixed)} captures need rewriting")

if "--apply" in sys.argv:
    for s in plan["steps"]:
        caps = s.get("capture") or {}
        for var, p in list(caps.items()):
            if p.startswith("$.data."):
                tail = p.split(".")[2:]
                key = (s["method"].upper(), norm(s["path"]))
                if key in scalar_routes and {t for _, t in scalar_routes[key]} <= {"Guid"} and tail == ["id"]:
                    caps[var] = "$.data"
    PLAN.write_text(json.dumps(plan, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    print(f"applied {len(fixed)} rewrites to {PLAN.name}")
else:
    print("(dry run - pass --apply to write the changes)")
