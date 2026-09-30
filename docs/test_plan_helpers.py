"""Regression tests for the helper logic inside docs/run_test_plan.py.

These cover the pieces that silently corrupt a run when they break:

  * pick()      - capture paths. A wrong path leaves a variable empty, and every later step
                  then sends a literal {{courseId}} and 404s.
  * resolve_obj - placeholder substitution, including object *keys* and the natural type of
                  a value that is exactly one placeholder.
  * with_query  - query-string merging. Dropping it makes 30 steps test the wrong request.

Run: python docs/test_plan_helpers.py
"""

import sys

sys.path.insert(0, "docs")
import run_test_plan as R  # noqa: E402

failures = []


def check(label, got, want):
    if got == want:
        print(f"  PASS  {label}")
    else:
        print(f"  FAIL  {label}\n          got:  {got!r}\n          want: {want!r}")
        failures.append(label)


ROLES = {"data": [
    {"roleId": "sa", "roleName": "SuperAdmin", "permissions": [{"id": "p1"}, {"id": "p2"}]},
    {"roleId": "in", "roleName": "Instructor", "permissions": [{"id": "p9"}]},
]}

print("pick() capture paths")
check("named filter", R.pick(ROLES, "$.data[roleName=Instructor].roleId"), "in")
check("wildcard", R.pick(ROLES, "$.data[*].roleId"), ["sa", "in"])
check("wildcard then property",
      R.pick({"data": {"permissions": [{"id": "a"}, {"id": "b"}]}}, "$.data.permissions[*].id"),
      ["a", "b"])
check("index", R.pick({"data": [{"id": "x"}, {"id": "y"}]}, "$.data[0].id"), "x")
check("bare data is a bare GUID", R.pick({"data": "01a0f3a6-3f51"}, "$.data"), "01a0f3a6-3f51")
check("empty list is [], not None",
      R.pick({"data": {"permissions": []}}, "$.data.permissions[*].id"), [])
check("missing name", R.pick(ROLES, "$.data[roleName=Nope].roleId"), None)
check("legacy dotted path", R.pick({"data": {"user": {"id": "u1"}}}, "$.data.user.id"), "u1")
check("legacy indexed path", R.pick({"data": {"items": [{"id": "i1"}]}}, "$.data.items[0].id"), "i1")

print("\nresolve_obj() placeholder substitution")
check("whole-body placeholder keeps list type",
      R.resolve_obj("{{ids}}", {"ids": '["a","b"]'}), ["a", "b"])
check("scalar placeholder stays a string", R.resolve_obj("{{id}}", {"id": "abc"}), "abc")
check("placeholder inside a list stays a string",
      R.resolve_obj(["{{id}}"], {"id": "abc"}), ["abc"])
check("object keys are substituted too",
      R.resolve_obj({"sectionOrders": {"{{sectionId}}": 2}}, {"sectionId": "s1"}),
      {"sectionOrders": {"s1": 2}})
check("embedded placeholder substitutes",
      R.resolve_obj("/x/{{id}}/y", {"id": "s1"}), "/x/s1/y")

print("\nwith_query() query-string merging")
r = R.Runner.__new__(R.Runner)
r.var = {"categoryId": "cat-1"}
check("scalars", r.with_query("/api/v1/Courses", {"pageNumber": 1, "pageSize": 10}),
      "/api/v1/Courses?pageNumber=1&pageSize=10")
check("values are URL-encoded", r.with_query("/x", {"searchTerm": "C#"}), "/x?searchTerm=C%23")
check("placeholder is resolved", r.with_query("/x", {"categoryId": "{{categoryId}}"}),
      "/x?categoryId=cat-1")
check("unresolved placeholder is dropped", r.with_query("/x", {"id": "{{missing}}"}), "/x")
check("empty value is dropped", r.with_query("/x", {"filePath": ""}), "/x")
check("no query object", r.with_query("/x", None), "/x")
check("merges with an existing query string",
      r.with_query("/x?status=Confirmed", {"pageSize": 1}), "/x?status=Confirmed&pageSize=1")
check("booleans are preserved, not dropped",
      r.with_query("/x", {"isAnswered": False, "isPublished": False}),
      "/x?isAnswered=False&isPublished=False")
check("separators inside values are encoded", r.with_query("/x", {"q": "a b&c=d"}),
      "/x?q=a%20b%26c%3Dd")

print("\nsql() must not return an error message as data")
try:
    R.sql("SELECT * FROM a_table_that_does_not_exist")
    print("  FAIL  a bad query raised nothing")
    failures.append("sql error detection")
except Exception as e:
    if "Msg" in str(e) or "not exist" in str(e).lower() or "Invalid object" in str(e):
        print(f"  PASS  bad query raises instead of returning text: {str(e)[:60]}")
    else:
        print(f"  FAIL  unexpected error type: {e}")
        failures.append("sql error type")

print("\nALL PASS" if not failures else f"\n{len(failures)} FAILURE(S): {failures}")
sys.exit(1 if failures else 0)