import json, pathlib

raw = pathlib.Path("docs/scalar-test-plan.json").read_text(encoding="utf-8")
plan = json.loads(raw)

print("### variables.adminPassword entry:")
print(json.dumps({"adminPassword": plan["variables"]["adminPassword"]}, indent=2))
print("### variables.adminEmail entry:")
print(json.dumps({"adminEmail": plan["variables"]["adminEmail"]}, indent=2))

for sid in ("S100", "S204", "S207"):
    s = next(x for x in plan["steps"] if x["id"] == sid)
    print(f"### step {sid}:")
    print(json.dumps(s, indent=2, ensure_ascii=False))

print("### howToUse.steps (raw list) count:", len(plan["howToUse"]["steps"]))
print("### version:", plan.get("version"))
print("### name:", plan.get("name"))
