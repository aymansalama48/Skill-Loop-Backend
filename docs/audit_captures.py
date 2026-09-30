"""Audit the plan's `capture` JSON paths against the real response schemas in the
OpenAPI document. A step that captures $.data.id from an endpoint whose data is a bare
GUID never sets its variable, and every later step that uses it silently sends the
literal {{courseId}}.

Run:  python docs/audit_captures.py <openapi.json>
"""
import json
import pathlib
import re
import sys

ROOT = pathlib.Path(__file__).resolve().parent.parent
PLAN = ROOT / "docs" / "scalar-test-plan.json"

TEMPLATE_RE = re.compile(r"\{\{[^}]*\}\}")
PARAM_RE = re.compile(r"\{[^}]*\}")


def norm(p):
    p = TEMPLATE_RE.sub("{}", p)
    p = PARAM_RE.sub("{}", p)
    return p.split("?", 1)[0].lower()


def main():
    doc = json.loads(pathlib.Path(sys.argv[1]).read_text(encoding="utf-8"))
    plan = json.loads(PLAN.read_text(encoding="utf-8"))

    def res(s, d=0):
        if d > 10 or not isinstance(s, dict):
            return {}
        if "$ref" in s:
            n = doc
            for p in s["$ref"].lstrip("#/").split("/"):
                n = n.get(p, {}) if isinstance(n, dict) else {}
            return res(n, d + 1)
        return s

    def envelope_for(op):
        """Walk the 200 response schema down to the object under `data`."""
        for code in ("200", "201", "204"):
            r = op.get("responses", {}).get(code)
            if not r:
                continue
            for c in r.get("content", {}).values():
                s = res(c.get("schema", {}))
                props = s.get("properties") or {}
                if "data" in props:
                    return res(props["data"]), s
                if "data" in (s.get("allOf") or []):
                    pass
                return None, s
        return None, {}

    problems = []
    checked = 0

    for step in plan["steps"]:
        caps = step.get("capture") or {}
        if not caps:
            continue
        key = f"{step['method'].upper()} {norm(step['path'])}"
        op = doc.get("paths", {}).get(step["path"].replace("{{", "{").replace("}}", "}"))
        if op is None:
            for p, methods in doc["paths"].items():
                if norm(p) == norm(step["path"]) and step["method"].lower() in methods:
                    op = methods[step["method"].lower()]
                    break
        if op is None:
            problems.append((step["order"], step["id"], "NO SCHEMA", "cannot verify"))
            continue

        data_schema, _ = envelope_for(op)
        if data_schema is None:
            continue  # endpoint returns no envelope, nothing to check

        checked += 1
        data_props = data_schema.get("properties")
        is_scalar = not data_props  # no declared properties -> treat as scalar/primitive

        for var, path in caps.items():
            if not path.startswith("$"):
                continue
            parts = [p for p in path.split(".") if p and p != "$"]
            if not parts or parts[0] != "data":
                continue
            tail = parts[1:]
            if not tail:
                continue  # $.data is correct for a scalar payload
            if is_scalar:
                problems.append(
                    (step["order"], step["id"], "BAD CAPTURE",
                     f"{var}: '{path}' but `data` is a scalar, not an object")
                )
            elif tail[0] not in data_props:
                problems.append(
                    (step["order"], step["id"], "BAD CAPTURE",
                     f"{var}: '{path}' but data has no '{tail[0]}' "
                     f"(has: {', '.join(list(data_props)[:8]) or 'none'})")
                )

    print(f"capture paths checked : {checked} steps with captures")
    print(f"problems              : {len(problems)}")
    for order, sid, kind, msg in problems:
        print(f"  [{order:>3}] {sid}  {kind}: {msg}")


if __name__ == "__main__":
    main()
