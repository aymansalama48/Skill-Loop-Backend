"""One-off patch: bring docs/scalar-test-plan.json in line with the API after the
origin/main refactor. Run once, then delete."""
import json, pathlib

P = pathlib.Path("docs/scalar-test-plan.json")
raw = P.read_text(encoding="utf-8")
plan = json.loads(raw)

steps = {s["id"]: s for s in plan["steps"]}

# --- S100: POST /api/v1/Courses is multipart now, not JSON -----------------------
s = steps["S100"]
s.pop("contentType", None)
s.pop("body", None)
s["form"] = {
    "Title": "C# Masterclass",
    "Description": "A complete guide to modern C#.",
    "Credits": 50,
    "Level": 2,
    "CategoryId": "{{categoryId}}",
}
s["optionalForm"] = {
    "ThumbnailImage": "C:/path/to/thumbnail.jpg",
}
s["notes"] = (
    "This endpoint takes multipart/form-data, not JSON - sending a JSON body returns 415. "
    "Level is the CourseLevel INTEGER: 1 Beginner, 2 Intermediate, 3 Advanced, 4 AllLevels; "
    "a string returns 400. There is no InstructorId/InstructorName field: the handler takes the "
    "instructor from the caller's token, so you cannot create a course on someone else's behalf "
    "unless the caller holds Courses.ManageAll. Gated by [Permissions.Courses.Create], so it "
    "needs the real staff token from S021."
)

# --- S204 / S207: these endpoints bind no request body ---------------------------
for sid, note in (
    ("S204",
     "Status travels as a QUERY parameter here, so the name 'Confirmed' is correct. The endpoint "
     "binds no request body - send none."),
    ("S207",
     "Run after S206. Once a session is Completed the cancellation window usually closes, so a 409 "
     "here is a valid result - check the reason in the response. The endpoint binds no request "
     "body - send none."),
):
    st = steps[sid]
    st.pop("body", None)
    st.pop("contentType", None)
    st["notes"] = note

# --- adminPassword is no longer a known value -------------------------------------
plan["variables"]["adminPassword"] = {
    "value": "",
    "source": "YOU MUST SUPPLY THIS - it is whatever you set in Seed:SuperAdmin:Password "
              "(dotnet user-secrets set \"Seed:SuperAdmin:Password\" \"...\"). There is no default "
              "password in the codebase any more.",
}
plan["variables"]["adminEmail"]["source"] = (
    "Seed:SuperAdmin:Email (dotnet user-secrets set ...). Defaults to admin@skillloop.com only "
    "if you seeded it yourself."
)

# --- how to run: the cloud agent trap ---------------------------------------------
steps_list = plan["howToUse"]["steps"]
plan["howToUse"]["steps"] = [
    "Start the API FIRST and leave that window open: "
    "powershell -ExecutionPolicy Bypass -File docs\\run-api-local.ps1 "
    "(it resolves the secrets, creates both databases, and switches Redis off).",
    "IMPORTANT - run the collection from the Postman DESKTOP app or the LOCAL agent. A Postman "
    "CLOUD agent executes on Postman's own servers, where 'localhost:7271' means the agent's "
    "machine and not yours, so every request fails before it reaches the API.",
    "The base URL is https://localhost:7271 (HTTPS, port 7271). Port 7000 only issues a 307 "
    "redirect to HTTPS, so pointing the collection at it makes every request look broken.",
] + steps_list[1:]

# --- scalar notes: Redis is optional now ------------------------------------------
plan["scalar"]["notes"] = [
    "Scalar is served in-process by the Scalar.AspNetCore package "
    "(src/Skill-Loop.Api/Extensions/OpenApiExtensions.cs). No Docker needed.",
    "docs/run-api-local.ps1 points SQL Server at the locally installed instance, creates the "
    "Skill-Loop and Skill-Loop-Hangfire databases, and disables Redis so the app falls back to "
    "the in-memory ICacheService. OTPs live in the database and rate limiting is in-process, so "
    "nothing else needs Redis.",
    "Both /scalar/v1 and /openapi/v1.json are registered only when ASPNETCORE_ENVIRONMENT="
    "Development. The script sets that for you.",
]

plan["version"] = "1.1.0"

P.write_text(json.dumps(plan, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
print("patched", P)
