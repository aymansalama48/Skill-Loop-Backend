# Testing Runbook - no Docker, no Redis

Two ways to exercise the API, both against the same 180-step plan.

- **Plan A - Postman collection runner** gives you a pass/fail summary. Use this for regression runs.
- **Plan B - Scalar** is for reading responses and poking single endpoints. Use this when something looks wrong.

Both need the API running first. Step 0 is shared.

---

## Step 0 - Start the API (once per session)

```powershell
cd C:\Users\Asus\Desktop\Skill-Loop-Backend
powershell -ExecutionPolicy Bypass -File docs\run-api-local.ps1
```

That single script does the five things that otherwise block startup on this laptop:

| Problem | What the script does |
| --- | --- |
| `Jwt:Key` and `OtpSettings:HashingSecret` are rejected at startup - the committed values in `appsettings.json` are treated as public on purpose | Reads them from .NET **user-secrets** (preferred), falling back to `.env` mapped to `Jwt__Key` / `OtpSettings__HashingSecret`. If neither has them, it fails with the exact `dotnet user-secrets set` commands to run |
| `appsettings.json` points at `AYMAN\MSSQLSERVER01`, which does not exist here | Overrides to `Server=localhost` - the default instance that *is* installed - using Windows integrated security, no password |
| The **Hangfire** database does not exist | Creates it. This one is not optional: EF Core would create the app database via `MigrateAsync()`, but Hangfire only ever creates its own *tables*, never the *database*. Without this, startup dies with `Cannot open database "Skill-Loop-Hangfire" requested by the login` |
| No Redis and no Docker | Sets `Redis__ConnectionString` to empty, which makes the app register its in-memory `ICacheService` instead of the Redis one. Nothing else needs Redis: OTPs live in the database, and rate limiting is in-process |
| Database does not exist yet | The script creates both `Skill-Loop` and `Skill-Loop-Hangfire` up front via `sp_executesql` (parameterised, so a quoted or dashed name is safe) |

Wait for this line:

```
Now listening on: https://localhost:7271
```

Leave that window open. Everything else happens in the browser.

Verify: <https://localhost:7271/scalar/v1> loads. First start takes ~20-40 seconds while migrations run.

> The HTTP port `7000` only ever answers `307` to redirect you to HTTPS. Test against `https://localhost:7271` directly, or follow the redirect - a `curl` against port `7000` will look like a broken endpoint.
>
> The dev HTTPS certificate is not trusted on this machine, so use `curl -k` (or accept the browser prompt once).

---

## Plan A - Postman collection runner

### A1. Import

1. Open Postman.
2. **Import** -> drag in `docs\skill-loop.postman_collection.json`.
3. You get a collection called **Skill Loop API Test Plan** with 19 phase folders and 180 requests.

### A2. Point it at your API

1. Open the collection -> **Variables** tab.
2. Set `baseUrl` to `https://localhost:7271`. It already defaults to this - only change it if you moved the port.

The first request must be sent once by hand so the collection learns the dev certificate: right-click the collection -> **Show code** is not needed, just run step 1 and accept the certificate prompt.

### A3. Set the file paths (7 requests)

Search the collection for `C:/path/to/` and point each at a real file on your machine. Any small file with the right extension works:

| Step | Field | Put anything like |
| --- | --- | --- |
| 54 - S082 | `file` | `C:/temp/avatar.png` |
| 57 - S090 | `iconFile` | `C:/temp/icon.png` |
| 58 - S091 | `iconFile` | `C:/temp/icon-v2.png` |
| 77 - S116 | `file` | `C:/temp/syllabus.pdf` |
| 78 - S117 | `file` | `C:/temp/lesson-slides.pdf` |
| 109 - S166 | `file` | `C:/temp/workshop.zip` |
| 167 - S280 | `file` | `C:/temp/probe.txt` |

Allowed extensions are enforced server-side (.png .jpg .pdf .zip .txt and similar). Max 10 MB.

### A4. Run

1. Right-click the collection -> **Run collection**.
2. **Deselect these 6 requests** - they need input you do not have yet, and they will fail:

   `9 - S014`, `13 - S018`, `14 - S019`, `49 - S071`, `50 - S072`, `51 - S073`

3. Click **Run**. It takes a few minutes for 174 requests.

### A5. Read the result

The runner shows a pass/fail count and an iteration table. Every request asserts its own expected status, so a red row is a real mismatch, not a script error.

- Expand a failed request -> **Check** to see actual vs expected status.
- Click the request -> **Console** for the response body.

**If you see 429 Too Many Requests:** the global limiter allows 300 requests/minute per IP (`RateLimitingExtensions.cs:43`). 174 in one burst is under that, but a second run straight after can trip it. Wait 60 seconds.

**If the first run fails on token-related steps:** the admin login (step 17) must succeed first. If it failed, everything after it has no token. Fix that one and re-run.

### A6. The 6 manual requests, afterwards

Now run them one at a time, filling in the variable first:

| Step | What to do |
| --- | --- |
| 9 - S014, 13 - S018 | Copy a published `courseId` out of the step 12 response, paste into the path. These are early smoke checks; S107 and S115 are the authoritative versions. |
| 14 - S019 | Set `promoCode` to `WELCOME10`. A 404 is the **correct** result - the code does not exist yet. S185 is the real validation, after S183 creates it. |
| 49 - S071 | Paste `invitationToken` from the step 48 (S070) response body, or from the invitation link in the email. |
| 50 - S072 | Accept the invitation with a password. **Mutually exclusive with 51** - invitations are single-use. |
| 51 - S073 | Only if you skipped 50. Re-send an invitation via S070 first, using the Google account's email address. |

---

## Plan B - Manual testing in Scalar

Use this when you want to read a response in full, or try a request by hand.

### B1. Open it

<https://localhost:7271/scalar/v1>

Left sidebar lists all 136 operations, grouped by controller, in 19 phases.

### B2. Authenticate

The API uses Bearer JWTs, and the OpenAPI document now advertises a `BearerAuth` security scheme, so:

1. Click the **Auth** tab at the top of the page.
2. Set the scheme to **BearerAuth**.
3. Paste a token into the value box.

**Where to get a token.** Log in as the seeded admin:

```
POST https://localhost:7271/api/v1/Auth/staff/login

{ "email": "admin@skillloop.com", "password": "Password@123" }
```

Copy `data.accessToken` from the response into Scalar's Auth box. Operations that need no token (the 28 anonymous ones) still work with the box empty - Scalar does not send the header unless you fill it.

Other roles exist and the plan creates them as it goes:

| Token | Who | How to get it |
| --- | --- | --- |
| `adminToken` | SuperAdmin | `Auth/staff/login` with the seeded credentials above |
| `userToken` | Student | `Auth/login` after registering + verifying an OTP |
| `instructorToken` | Instructor | Register, apply, get approved by admin, then log in |

### B3. Run a request

1. Click the operation in the sidebar.
2. Fill in path params, query params and the body.
3. Click **Test Request**.

**Which auth each operation needs** - Scalar shows this per operation:

- **108 secured** operations show a lock and inherit the `BearerAuth` requirement.
- **28 anonymous** operations are explicitly marked `security: []` and send no header.

### B4. Working through the plan in Scalar

Scalar is for inspection, not for the full 180-step chain, because it has no variable substitution between requests - you copy ids by hand. A workable middle path:

1. Run the Postman collection first (Plan A) so the data exists: categories, courses, sections, sessions, bookings.
2. Come to Scalar to read and verify interesting responses in full.
3. For anything that looks like a bug, reproduce it there and check the fields you actually care about.

### B5. Useful things to poke at

- **Enum values are integers in JSON bodies.** There is no `JsonStringEnumConverter` registered, so `materialType` is `0`, not `"PDF"`. Sending `"PDF"` in a JSON body gets a 400. (Form fields are different - there `PDF` is fine, because the string is bound by name.)
- **User and quick-login tokens carry empty permission claims.** If a permission-gated call returns 403 for a normal user, that is expected - use the admin token, or do a refresh-token round trip to pick up the real claims.
- **Rate limits** are 300/min global, 5/min per login email, 3 OTP verifies per 10 min. Hitting those returns 429 with a `Retry-After` header.

---

## Regenerating the artifacts

Both files are generated. To change the plan, edit `docs/scalar-test-plan.json` then:

```powershell
python docs\generate_test_artifacts.py
```

Never hand-edit `api-tests.http` or the Postman collection - the next run overwrites them.
