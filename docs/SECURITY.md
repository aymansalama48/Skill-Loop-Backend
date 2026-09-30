# Security Policy & Operational Runbook

This document covers the security controls in Skill Loop, how to configure them safely, and
what to do when a secret is exposed.

Reporting a vulnerability: see [Reporting a Vulnerability](#reporting-a-vulnerability).

---

## 1. Immediate action required: rotate the committed secrets

The following values were committed to `src/Skill-Loop.Api/appsettings.json` and are in the
Git history. **They must be treated as public.** Sanitising the current file is not enough —
removing a line from a file does not remove it from history.

| Secret | Config key | Impact if leaked |
|---|---|---|
| JWT signing key | `Jwt:Key` | Anyone can mint a valid access token for **any** user id, including `SuperAdmin`. Total authentication bypass. |
| OTP hashing secret | `OtpSettings:HashingSecret` | Derives the per-purpose OTP HMAC key. Allows forging verification codes for password reset and email confirmation. |
| SMTP password | `MailSettings:Password` | Mail account compromise; phishing from your domain. |
| SuperAdmin credentials | `Seed:SuperAdmin:Password` (was hardcoded `Password@123`) | Direct administrative access. |

### Rotation checklist

1. **Generate new secrets.** Never reuse a value from the repository.

   ```bash
   # Windows (PowerShell)
   [Convert]::ToBase64String((1..48 | ForEach-Object { Get-Random -Maximum 256 }))
   # or, preferably, from an OS CSPRNG:
   dotnet user-secrets set "Jwt:Key" "$(openssl rand -base64 48)"
   ```

2. **Roll out the new values.**

   | Environment | Mechanism |
   |---|---|
   | Local development | `dotnet user-secrets` (stored outside the repo, encrypted per-user) |
   | Staging / Production | Environment variables using `__` as the section separator, e.g. `Jwt__Key`, `OtpSettings__HashingSecret`, `MailSettings__Password` |

3. **Expect immediate session invalidation.** Changing `Jwt:Key` invalidates every issued
   access token. Applying the `HardenRefreshTokenStorage` migration deletes all stored
   refresh tokens, so **all users must sign in again**. This is intended — see §3.3.

4. **Rotate the SMTP password** in the mail provider's control panel as well as in
   configuration. The old one must be revoked, not just replaced.

5. **Purge the values from Git history.** The history rewrite is destructive and forces every
   collaborator to re-clone. Coordinate before running it.

   ```bash
   # 1. Confirm the value is really in history
   git log --all -S 'kjh4' --oneline

   # 2. Rewrite (install git-filter-repo first: pip install git-filter-repo)
   git filter-repo --invert-paths \
     --path src/Skill-Loop.Api/appsettings.json

   # 3. Force-push all branches and tags, then have everyone re-clone
   git push --force --mirror
   ```

   Alternatively use [BFG Repo-Cleaner](https://rtyley.github.io/bfg-repo-cleaner/).

6. **Rotate the SuperAdmin password** and review the account's recent activity (sign-in
   notifications, role changes, permission grants). Assume any access before the rotation
   was legitimate-but-compromised.

> **A committed secret is compromised.** History rewriting reduces future exposure; it does
> not undo the fact that the value was readable. Rotation is the actual fix. Treat every
> secret that has ever been in this repository as permanently burned.

---

## 2. Configuration requirements

`src/Skill-Loop.Api/appsettings.json` is **tracked in Git and must never contain secrets.**
It ships with empty values and inline comments explaining each key.

`SecurityConfigurationExtensions.AddValidatedSecurityConfiguration` runs at startup and
**refuses to boot** when the configuration is unsafe. All problems are reported at once so a
deployment does not need a fix-redeploy cycle per issue.

| Requirement | Rule |
|---|---|
| `Jwt:Key` | Required, ≥ 32 characters. HMAC-SHA256 keys shorter than 32 bytes are not safe to sign with. |
| `Jwt:ExpiryMinutes` | `> 0` and `≤ 60`. A longer lifetime widens the damage window of a leaked access token. |
| `OtpSettings:HashingSecret` | Required, ≥ 32 characters. Previously committed values are explicitly rejected. |
| `OtpSettings:ResendCooldown` | Parseable `TimeSpan`, `≥ 00:01:00`. |
| `CorsSettings:AllowedOrigins` | Non-empty. `*` is rejected (see §4.3). |
| `ConnectionStrings:DefaultConnection` | Required. |
| `ConnectionStrings:HangfireConnection` | Required, and must be a **separate database** (see §5.2). |

Placeholder-looking values (anything starting with `your`, containing `placeholder`, or
matching a known compromised constant) are rejected at startup.

---

## 3. Authentication controls

### 3.1 Access tokens

- **HMAC-SHA256**, signed with `Jwt:Key`. The generator throws rather than sign with a missing
  or short key.
- **`exp`, `nbf`, and `iat` are UTC.** `DateTime.UtcNow` is used, never display-local time.
  The previous implementation used Egypt local time, which shifted every token's real lifetime
  by the UTC offset — and by an extra hour during Egypt DST.
- **60-minute maximum lifetime**, enforced at startup.

### 3.2 Password reset revokes all sessions

A password reset that leaves existing refresh tokens valid is not account recovery: an
attacker holding a stolen token can reset the victim's password — locking the legitimate user
out — and remain fully authenticated for the remaining token lifetime.

`ResetPasswordCommandHandler` therefore calls `RevokeAllUserTokensAsync` after a successful
reset. If revocation itself fails, the command still succeeds (the password *was* changed) and
the failure is logged at `Error` level rather than swallowed.

Account deactivation already revoked sessions; that path is unchanged.

### 3.3 Refresh token storage and rotation

Refresh tokens are bearer credentials valid for up to 7 days, so they are stored **only as a
SHA-256 digest**. The plaintext is returned to the client once and never persisted.

- **Lookup** hashes the presented token and compares digests in constant time, mirroring
  `OtpService`.
- **Rotation** issues a successor and links the two via `TokenFamilyId` /
  `ReplacedByTokenHash`.
- **Reuse detection**: presenting an already-rotated token means it leaked. The entire token
  family is revoked and the caller receives `TOKEN_REUSE_DETECTED`. This is scoped per family
  so one compromised token cannot log out every user.
- **Account state is re-checked** on every refresh: a deactivated or unconfirmed account
  cannot use a valid token to re-enter.
- **`TokenFamilyId` is never `Guid.Empty`.** That would collapse every session into one family
  and turn a single replayed token into a system-wide denial of service.

**Migration impact:** `HardenRefreshTokenStorage` deletes all existing refresh tokens. The old
column held the plaintext token, so a digest cannot be back-filled without reading the value
being removed. Rotating is the correct trade-off: those sessions are assumed compromised
precisely because the plaintext was stored. **All users must sign in again after deployment.**

SHA-256 rather than a slow KDF is deliberate: the input is 256 bits of CSPRNG output, so
there is no dictionary to brute-force, and a deliberate KDF would add latency to every refresh.

### 3.4 Rate limiting

The application previously had no throttling, so every auth endpoint was an unlimited oracle.
Identity's lockout only caps attempts per account, so a distributed attacker still got five
guesses per account from every source address — and could turn lockout into a denial of
service against a chosen target.

`RateLimitingExtensions` provides a global per-IP limit plus targeted policies. Partitions
combine the remote address **and** the identity being targeted, so an attacker can neither
lock out all users nor bypass a per-account limit by rotating addresses.

| Policy | Limit | Applies to |
|---|---|---|
| `global` | 300 / min per IP | Everything, including unlisted endpoints |
| `login` | 5 / min per IP + email | Staff & user login, Google login |
| `otp-verify` | 3 / 10 min per IP + email | OTP verify, password reset |
| `otp-resend` | 2 / 10 min per IP + email | OTP resend, registration, forgot password |
| `email-test` | 5 / min per IP | Admin test email |
| `contact-form` | 3 / 10 min per IP | Support contact form |
| `refresh-token` | 20 / min per IP | Token refresh |

The identity is read from the JSON request body by `RateLimitIdentityMiddleware`, which buffers
and rewinds the stream so model binding is unaffected. Malformed or oversized bodies fall back
to IP-only partitioning.

Rejected requests return `429` with `Retry-After`.

### 3.5 OTP

Codes are HMAC-hashed with a key bound to both the identifier **and the purpose**, so a code
issued for email confirmation cannot validate against a password-reset row even if the two
digits happen to collide. Cooldown and max-attempt limits are enforced per identifier.

All OTP comparisons use `UtcNow`. The resend cooldown previously compared a UTC timestamp
against the display-local clock, which made a configured 60-second cooldown block resend for
the full UTC offset — hours instead of a minute, and a denial of service against the
legitimate user trying to receive their own code.

### 3.6 UTC discipline

`IDateTime` exposes two members with deliberately different meanings:

| Member | Use for |
|---|---|
| `UtcNow` | Anything persisted, compared, or protocol-facing: token expiry, `*Utc` columns, audit timestamps, cooldown windows, scheduling |
| `Now` | Formatting a timestamp for display only |

`Now` returns Egypt local time. Comparing a stored UTC value against it shifts every decision
by the UTC offset — and by an extra hour during Egypt DST. This was a systemic defect; the
sites corrected as part of the security review are:

| Site | Effect of the bug |
|---|---|
| `JwtTokenGenerator` | Every token's real lifetime shifted by the offset |
| `OtpService` (expiry, cooldown, `VerifiedAt`) | Cooldown blocked for hours; expired codes stayed valid |
| `InvitationService` (`ExpiresAtUtc`, `UsedAtUtc`) | Staff invitations stayed valid 2 hours past expiry — an auth credential outliving its intended window |
| `StaffInvitationCreatedEventHandler` | Wrong remaining-validity hours in the invitation email |
| `UserAuthService` / `StaffAuthService` (`LastLoginAt`, `LoggedInAt`) | Login audit trail skewed; incorrect values for incident review |
| `CreateBooking` / `CancelBooking` / `ChangeBookingStatus` | Sessions looked 2 hours further out than they were, allowing booking of already-started sessions; booking transitions persisted a 2-hour skew |
| `AuditableEntityInterceptor` / `SoftDeleteInterceptor` | Baseline `CreatedAt` / `UpdatedAt` / `DeletedAt` for every entity skewed |
| `InsertOutboxMessagesInterceptor` / `ProcessOutboxMessagesJob` | Outbox message ordering and retry windows skewed |

**Not changed, needs a product decision:**

- `GetMyEarningsSummaryQueryHandler` builds "this month" boundaries from the display-local
  calendar and then labels them `DateTimeKind.Utc`. Which timezone defines a reporting month
  is a business decision, not a security fix. The mixed `Kind` should be resolved deliberately.
- `BookingEmailHandler` reads `Now.Year` purely to stamp the year into an email template. That
  is a display concern, so local time is correct here.

### 3.6 Bootstrap SuperAdmin

There is no default admin account and no password in code. `ContextSeed.SeedSuperAdminAsync`
creates the account only when `Seed:SuperAdmin:Email` **and** `Seed:SuperAdmin:Password` are
both configured, and it never overwrites an existing account. Without a configured password
the seed is skipped with an `Error` log — a deployment gets no admin account rather than a
guessable one.

---

## 4. Authorization controls

### 4.1 Permission-gated admin surfaces

`AdminEmailsController` is gated by `[Authorize(Roles = "Admin,SuperAdmin")]` in addition to
the per-command `[Permission]` check. A bare `[Authorize]` previously let any authenticated
user reach the controller, which combined with an unrestricted recipient to expose the
platform SMTP relay as an open spam relay.

- **Test email** requires `Emails.SendTest` and the recipient is restricted to the configured
  `SiteSettings.SupportEmail`.
- **Resend** requires `Emails.Resend` and is limited to `TestEmail`; security-sensitive email
  types are rejected.
- **Site settings updates** require `SiteSettings.Update` plus `SuperAdmin` on the route.

### 4.2 Fail-closed identity resolution

`BaseApiController.RequireUserId()` replaces the `_currentUser.UserId ?? Guid.Empty` pattern
across eight controllers. `Guid.Empty` is a real, non-null value, so it flowed into commands
and handlers ran their queries and writes against a user that did not exist — producing either
silent data loss or rows persisted under a shared sentinel identity. A missing user-id claim
on an authorized endpoint is an authentication failure, so it now returns `401` and the
command is never constructed.

### 4.3 CORS

The policy uses `AllowCredentials` with an explicit origin allowlist. A wildcard is never
valid here: combined with credentials, `*` would let any website read authenticated API
responses. An empty allowlist is rejected rather than defaulted to a localhost origin, which
would hide a misconfiguration until it becomes a production outage. In non-Development
environments, loopback and plain-HTTP origins are rejected at startup.

### 4.4 Dev-only controllers

Controllers under the `Skill_Loop.Api.Controllers.Dev` namespace are removed from the MVC
application model outside Development, so they are absent from the routing table and OpenAPI
document entirely. A per-action `IsDevelopment()` check is not sufficient: if
`ASPNETCORE_ENVIRONMENT` is misconfigured, the endpoints would still be routed.

---

## 5. Infrastructure and transport

### 5.1 Security headers

`SecurityHeadersMiddleware` sets `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`,
`Referrer-Policy`, a restrictive `Content-Security-Policy`, and removes `X-Powered-By`.

`nosniff` is the important one: user-supplied HTML is stored (course descriptions, email log
bodies, uploaded material names). Without it, a browser may sniff an uploaded file as HTML and
execute it in the site's origin. HSTS is enabled in non-Development environments.

### 5.2 Hangfire database isolation

`ConnectionStrings:HangfireConnection` is required and must point at a **different database**
from the application. Sharing one database means the background worker competes with live
requests for the same connection pool and locks, and a runaway job can block application
queries. Worker count is clamped to `[2, 20]`.

### 5.3 Input validation

`UpdateSiteSettingsCommandValidator` enforces bounded lengths and accepts only absolute
`http`/`https` URLs. The previous validator used FluentValidation's `Must(...)` overload, which
takes an `Expression` that is **not evaluated** — so `not-a-valid-url` was accepted. Site
settings URLs are rendered as links in outbound email, making this an injection vector.

### 5.4 Logging and correlation IDs

`CorrelationIdMiddleware` accepts a client-supplied correlation ID only if it is ≤ 64
characters and purely alphanumeric, `-`, `_`, or `.`. Otherwise it generates a new GUID. An
untrusted value is echoed into the response header and every log line for the request, so
accepting it raw allowed log forging via CR/LF and unbounded log growth.

`GlobalExceptionHandler` maps `UnauthorizedAccessException` to `401` (logged at `Warning`, not
`Error`, since a spike usually indicates a broken token or middleware ordering bug rather than
an attack) and `DbUpdateConcurrencyException` to `409`.

### 5.5 Transactional integrity

`NonGenericCommandTransactionBehavior` adds transactional coverage for commands that do not
return a payload (`ICommand` without `TResponse`). The existing `TransactionBehavior` was
constrained to `ICommand<TResponse>` and therefore never matched them, so commands such as
`DeactivatePromoCode` and `AssignPermissionToRole` ran with no transaction at all — a partial
write could leave a role holding a subset of the permissions that were requested.

---

## 6. Security regression tests

`dotnet test tests/Skill-Loop.UnitTests`

| Suite | Covers |
|---|---|
| `External/Security/RefreshTokenServiceTests` | Digest-only storage, per-login token families, rotation, reuse detection scoped per family, deactivated/unconfirmed rejection, UTC expiry comparison, revocation |
| `Features/Accounts/.../ResetPasswordCommandHandlerTests` | Sessions revoked on reset; not revoked on failed OTP or failed password change; revocation failure does not fail the command |
| `Api/Security/SecurityConfigurationExtensionsTests` | Missing/short/committed-placeholder secrets rejected; wildcard and loopback CORS rejected; all problems reported together |
| `Api/Middlewares/CorrelationIdMiddlewareTests` | CR/LF and oversized correlation IDs rejected, safe values preserved |
| `Features/SiteSettings/.../UpdateSiteSettingsCommandValidatorTests` | `javascript:`, `//host`, `ftp://`, and overlong values rejected |

---

## 7. Reporting a Vulnerability

Report vulnerabilities privately to the repository maintainers. Please include:

- description and impact,
- reproduction steps or a proof of concept,
- affected version or commit.

Please do **not** open a public issue for an unfixed vulnerability, and do not test against
accounts or data you do not own.

---

## 8. Related documentation

| Doc | Covers |
|-----|--------|
| [README.md](../README.md) | Setup, configuration, endpoint inventory |
| [ARCHITECTURE_GUIDE.md](ARCHITECTURE_GUIDE.md) | Layer-by-layer walkthrough |
| [explanation.md](explanation.md) | High-level code map and request flows |
| [TESTING_RUNBOOK.md](TESTING_RUNBOOK.md) | Running and writing tests |
