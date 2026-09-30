"""
Audits docs/scalar-test-plan.json against the live OpenAPI document.

Reports, per planned step:
  - method/path that the API does not expose
  - a JSON body that is missing a property the schema marks required
  - a JSON body property the schema does not define (typo / renamed field)

Run:  python docs/audit_plan_against_openapi.py [path-to-openapi.json]
"""
import json
import pathlib
import re
import sys

ROOT = pathlib.Path(__file__).resolve().parent.parent
PLAN = ROOT / "docs" / "scalar-test-plan.json"

TEMPLATE_RE = re.compile(r"\{\{[^}]*\}\}")
PARAM_RE = re.compile(r"\{[^}]*\}")


def norm(path: str) -> str:
    """Collapse every path parameter to {} so plan and spec shapes compare equal."""
    path = TEMPLATE_RE.sub("{}", path)
    path = PARAM_RE.sub("{}", path)
    path = path.split("?", 1)[0]
    return path.lower()


def resolve(schema, doc, depth=0):
    """Follow $ref until we reach a real schema object."""
    if depth > 10 or not isinstance(schema, dict):
        return {}
    if "$ref" in schema:
        node = doc
        for part in schema["$ref"].lstrip("#/").split("/"):
            node = node.get(part, {}) if isinstance(node, dict) else {}
        return resolve(node, doc, depth + 1)
    return schema


def properties_of(schema, doc):
    """Return (properties dict, required list) for an object schema, allOf merged."""
    schema = resolve(schema, doc)
    props, required = {}, []

    for sub in schema.get("allOf", []) or []:
        p, r = properties_of(sub, doc)
        props.update(p)
        required.extend(r)

    props.update(resolve(schema.get("properties", {}), doc))
    required.extend(schema.get("required", []) or [])

    if schema.get("additionalProperties") is False and not props and not schema.get("allOf"):
        # closed schema with no declared members; caller treats this as "unknown type"
        props = None

    return props, required


def main():
    openapi_path = pathlib.Path(sys.argv[1]) if len(sys.argv) > 1 else None
    if openapi_path is None:
        import urllib.request
        raise SystemExit("pass the path to a downloaded openapi/v1.json")

    doc = json.loads(openapi_path.read_text(encoding="utf-8"))
    plan = json.loads(PLAN.read_text(encoding="utf-8"))

    ops = {}
    for path, methods in doc.get("paths", {}).items():
        for method, op in methods.items():
            if method.lower() in {"get", "post", "put", "delete", "patch"}:
                ops[f"{method.upper()} {norm(path)}"] = op

    missing, body_issues, ct_issues = [], [], []

    for step in plan["steps"]:
        key = f"{step['method'].upper()} {norm(step['path'])}"
        op = ops.get(key)

        if op is None:
            # A literal value in place of a parameter (e.g. WELCOME10) still matches a
            # templated route, so retry with that segment collapsed too.
            alt = f"{step['method'].upper()} {norm(step['path'].replace('WELCOME10', '{{promoCode}}'))}"
            op = ops.get(alt)

        if op is None:
            missing.append((step["order"], step["id"], key))
            continue

        content = op.get("requestBody", {}).get("content", {})
        wants_form = "multipart/form-data" in content
        wants_json = any(c.startswith("application/json") for c in content)

        body = step.get("body")
        form = step.get("form") or step.get("optionalForm")

        if form and wants_json and not wants_form:
            ct_issues.append((step["order"], step["id"], "plan sends multipart, API expects JSON"))
        elif body and wants_form:
            ct_issues.append((step["order"], step["id"], "plan sends JSON, API expects multipart/form-data"))

        if not isinstance(body, dict):
            continue

        schema = None
        for ctype in ("application/json", "multipart/form-data"):
            if ctype in content:
                schema = content[ctype].get("schema")
                break
        if schema is None:
            continue

        props, required = properties_of(schema, doc)
        if props is None:
            continue

        for name in body:
            if name not in props:
                body_issues.append((step["order"], step["id"], "UNKNOWN PROP", name))
        for name in required:
            if name not in body:
                body_issues.append((step["order"], step["id"], "MISSING REQUIRED", name))

    print(f"planned steps : {len(plan['steps'])}")
    print(f"live endpoints: {len(ops)}")
    print()
    print(f"=== routes not exposed by the API ({len(missing)}) ===")
    for order, sid, key in missing:
        print(f"  [{order:>3}] {sid}  {key}")
    print()
    print(f"=== body content-type mismatches ({len(ct_issues)}) ===")
    for order, sid, msg in ct_issues:
        print(f"  [{order:>3}] {sid}  {msg}")
    print()
    print(f"=== body property mismatches ({len(body_issues)}) ===")
    for order, sid, kind, name in body_issues:
        print(f"  [{order:>3}] {sid}  {kind}: {name}")


if __name__ == "__main__":
    main()
