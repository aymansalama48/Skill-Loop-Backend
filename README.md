<p align="center">
  <h1 align="center">🔄 Skill Loop</h1>
  <p align="center">
    <strong>A modern skill-sharing and management platform — built with Clean Architecture on .NET 10</strong>
  </p>
  <p align="center">
    <img src="https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 10" />
    <img src="https://img.shields.io/badge/C%23-13-239120?style=for-the-badge&logo=csharp&logoColor=white" alt="C#" />
    <img src="https://img.shields.io/badge/SQL%20Server-2022-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white" alt="SQL Server" />
    <img src="https://img.shields.io/badge/License-MIT-yellow?style=for-the-badge" alt="License" />
  </p>
</p>

---

## 📖 Overview

**Skill Loop** is a graduation-project backend API that powers a skill-sharing platform. It enables users to discover, learn, and share skills with each other through a robust, enterprise-grade REST API. The system supports multi-role staff management with a granular permission system, invitation-based onboarding, OTP verification, and Google OAuth integration.

---

## ✨ Key Features

| Category | Features |
|---|---|
| 🔐 **Authentication** | Staff Login (Email/Password) · Google OAuth · JWT Access & Refresh Tokens · Secure Logout |
| 👤 **Account Management** | Profile CRUD · Avatar Upload · Password Change · Forgot/Reset Password · Activate/Deactivate Users |
| 🎤 **Instructor Profiles** | Create/Update Profile · Approval Workflow · Availability Slots · Reviews & Ratings · Instructor Stats (Sessions Completed, Credits Earned) |
| 💰 **Wallets** | View Balance · Transaction History (Paginated) · Auto-Credit on Course Enrollment · Optimistic Concurrency |
| 📚 **Courses & Enrollments** | Course Create & Publish · Sections & Lessons · Course Reviews · Bookmarks · Enrollment · Lesson Progress Tracking |
| 🎓 **Sessions & Materials** | Session CRUD · Status Management · File Upload/Download (Google Drive) · Material Reordering |
| 💬 **Real-time Chat** | 1-on-1 Conversations · SignalR WebSocket · Read Receipts · Message Notifications |
| 🛟 **Support & FAQ** | Public FAQ Search · Authenticated Contact Requests · Staff Answering & Publishing · Email Notifications |
| 🔔 **In-App Notifications** | Notification feed (paginated) · Unread count · Mark one/all as read |
| 📩 **Staff Invitations** | Admin sends invite via email → Staff accepts with password or Google account |
| 🔑 **Permission System** | Granular module-based permissions · Role–Permission assignment · Dynamic RBAC |
| 📱 **OTP Verification** | HMAC-hashed codes · Configurable expiry & cooldown · Max attempts lockout |
| 🎟️ **Live Session Booking** | Book sessions · Cancel with refund · Instructor status management · Credit payment |
| 🛡️ **Security** | Hashed refresh tokens with reuse detection · Per-endpoint rate limiting · Session revocation on password reset · Fail-fast secret validation · Security headers · Strict CORS allowlist |
| 📊 **Observability** | Structured logging (Serilog) · Validated correlation IDs · Performance tracking |
| ⚙️ **Background Jobs** | Outbox pattern with Hangfire (dedicated database) for reliable domain event processing |

---

## 🏗️ Architecture

The project follows **Clean Architecture** (aka Onion Architecture) with strict dependency rules:

```
┌─────────────────────────────────────────────────┐
│                  Skill-Loop.Api                  │  ← Presentation (Controllers, Middlewares)
├─────────────────────────────────────────────────┤
│             Skill-Loop.Application               │  ← Use Cases (CQRS via MediatR)
├─────────────────────────────────────────────────┤
│               Skill-Loop.Domain                  │  ← Core Entities, Enums, Domain Events
├─────────────────────────────────────────────────┤
│           Skill-Loop.Infrastructure              │  ← EF Core, Identity, Email, Jobs, Cache
└─────────────────────────────────────────────────┘
```

### Layer Responsibilities

- **Domain** — Pure business entities (`Course`, `InstructorProfile`, `Session`, `UserWallet`, `StaffInvitation`, `OtpVerification`), base entity types (`AuditableEntity`, `SoftDeleteEntity`), domain events, enums, and the `Result<T>` pattern for error handling.
- **Application** — Commands & Queries (CQRS) via MediatR, FluentValidation, AutoMapper, and a rich pipeline of cross-cutting behaviors (Logging → Performance → Authorization → Validation → Caching → Cache Invalidation → Transaction).
- **Infrastructure** — EF Core with SQL Server, ASP.NET Core Identity, JWT token management, Google Auth, email (MailKit/SMTP), file storage (local + Google Drive), Hangfire background jobs, Outbox pattern, and cache-aside caching (in-memory + Redis).
- **Api** — ASP.NET Core controllers, request/response contracts, global exception handling, correlation ID middleware, Serilog integration, SignalR (real-time chat), and Scalar (OpenAPI) documentation.

---

## 🧰 Tech Stack

### Frameworks & Runtime
- **.NET 10** / **C# 13**
- **ASP.NET Core 10** (Minimal Hosting)

### Data & Persistence
- **Entity Framework Core 10** (Code-First)
- **SQL Server** (via `Microsoft.EntityFrameworkCore.SqlServer`)
- **ASP.NET Core Identity** (Users, Roles, Claims)

### Application Patterns
- **MediatR 14** — CQRS (Command/Query segregation)
- **FluentValidation 12** — Input validation
- **AutoMapper 16** — Object mapping

### Authentication & Security
- **JWT Bearer Tokens** (`Microsoft.AspNetCore.Authentication.JwtBearer`) — HMAC-SHA256, UTC `exp`/`iat`/`nbf`
- **Rotating refresh tokens** — SHA-256 digest storage, token families, reuse detection
- **Endpoint rate limiting** (`Microsoft.AspNetCore.RateLimiting`) — per-IP and per-identity
- **Google OAuth** (`Google.Apis.Auth`)
- **OTP with HMAC Hashing** — purpose-derived key separation

### Infrastructure
- **Hangfire** — Background job processing & outbox consumer
- **MailKit** — SMTP email delivery
- **Serilog** — Structured logging (Console + File sinks + enrichers)
- **StackExchange.Redis** — Distributed cache (alongside the in-memory cache)
- **UAParser** — User-agent detection

### API Documentation
- **Scalar** (OpenAPI/Swagger alternative)

---

## 📂 Project Structure

```
Skill-Loop/
├── Skill-Loop.slnx                    # Solution file
├── README.md
│
├── src/
│   ├── Skill-Loop.Api/                # 🌐 Presentation Layer
│   │   ├── Controllers/               #   API endpoints
│   │   │   ├── AuthController           #     Login, Google login, register, OTP, refresh, logout
│   │   │   ├── ProfileController        #     Current user profile
│   │   │   ├── UsersController          #     Admin user management
│   │   │   ├── StaffInvitationsController  # Invitation flow
│   │   │   ├── PermissionManagementController  # RBAC management
│   │   │   ├── CoursesController       #     Courses CRUD
│   │   │   ├── EnrollmentsController   #     Course enrollment & progress
│   │   │   ├── SessionsController      #     Sessions CRUD
│   │   │   ├── SessionMaterialsController  # Session file management
│   │   │   ├── InstructorProfilesController  # Instructor profiles & reviews
│   │   │   ├── WalletsController       #     Wallet balance & transactions
│   │   │   ├── ChatController          #     Real-time chat
│   │   │   ├── SupportController       #     Public FAQ + user contact requests
│   │   │   ├── SupportManagementController  # Staff support queue (answer/publish/delete)
│   │   │   ├── NotificationsController #     In-app notifications
│   │   │   ├── SiteSettingsController  #     Public site settings (SuperAdmin update)
│   │   │   ├── DevController           #     Development-only helpers (removed from routing outside Development)
│   │   │   └── BookingsController      #     Live session bookings
│   │   ├── Contracts/                  #   Request/Response DTOs
│   │   ├── Middlewares/                #   CorrelationId, SecurityHeaders, GlobalExceptionHandler
│   │   ├── Extensions/                 #   Pipeline, DI, CORS, RateLimiting, SecurityConfiguration
│   │   └── appsettings.json           #   Configuration (tracked, contains NO secrets)
│   │
│   ├── Skill-Loop.Application/        # 📋 Application Layer
│   │   ├── Features/
│   │   │   ├── Accounts/              #   Auth, Account Mgmt, Permissions, Invitations
│   │   │   ├── Categories/            #   Categories Commands & Queries
│   │   │   ├── Courses/               #   Course Commands, Queries & Events
│   │   │   ├── Enrollments/           #   Enrollment & Lesson Progress
│   │   │   ├── Instructors/           #   Instructor Profile CRUD, Reviews, EventHandlers, Availability
│   │   │   ├── Wallets/               #   Wallet Queries, DTOs & EventHandlers
│   │   │   ├── Sessions/              #   Sessions & Materials Commands & Queries
│   │   │   ├── Chat/                  #   Chat Commands & Queries
│   │   │   ├── Support/               #   FAQ & support requests Commands/Queries
│   │   │   ├── Notifications/         #   In-app notifications Commands/Queries
│   │   │   ├── SiteSettings/          #   Site settings Commands/Queries
│   │   │   └── Bookings/              #   Live session booking commands & queries
│   │   │
│   │   │   (OTP verification is not a separate feature folder — `OtpService` lives in Infrastructure and is used by the Accounts user-auth commands.)
│   │   ├── Common/
│   │   │   ├── Behaviors/             #   MediatR pipeline behaviors
│   │   │   ├── Abstractions/          #   Service interfaces
│   │   │   ├── Pagination/            #   Paginated result helpers
│   │   │   ├── Validation/            #   Shared validators
│   │   │   └── Errors/               #   Error constants
│   │   └── DependencyInjection.cs     #   Application DI registration
│   │
│   ├── Skill-Loop.Domain/            # 🏛️ Domain Layer
│   │   ├── Entities/
│   │   │   ├── Booking/               #   Booking entity
│   │   │   ├── Chat/                  #   Conversation, ChatMessage
│   │   │   ├── Courses/               #   Course aggregate, Value Objects, Events
│   │   │   ├── Enrollments/           #   Enrollment, LessonProgress, Events
│   │   │   ├── Instructors/           #   InstructorProfile, InstructorReview
│   │   │   ├── Invitation/            #   StaffInvitation + domain events
│   │   │   ├── OtpVerification/       #   OTP entity
│   │   │   ├── Session/               #   Session, SessionMaterial, Events
│   │   │   ├── SiteSettings/          #   SiteSettings entity
│   │   │   ├── Support/               #   SupportQuestion entity
│   │   │   ├── Notifications/         #   Notification entity
│   │   │   └── Wallets/               #   UserWallet, WalletTransaction, Events
│   │   ├── Common/
│   │   │   ├── Entities/              #   BaseEntity, AuditableEntity, SoftDeleteEntity
│   │   │   ├── Events/               #   Domain event base types
│   │   │   └── Results/              #   Result<T>, Error, ErrorType
│   │   ├── Enums/                     #   OtpPurpose, SessionStatus, BookingStatus, etc.
│   │   └── Constants/                 #   Roles, Permissions
│   │
│   └── Skill-Loop.Infrastructure/     # 🔧 Infrastructure Layer
│       ├── Persistence/
│       │   ├── Data/                  #   AppDbContext, migrations factory
│       │   ├── IdentityModels/        #   ApplicationUser, ApplicationRole, RefreshToken, Permissions
│       │   ├── Configurations/        #   EF Core entity configurations
│       │   ├── Interceptors/          #   SaveChanges interceptors
│       │   ├── Outbox/                #   Outbox message pattern
│       │   ├── Seed/                  #   Database seeder
│       │   └── Transaction/           #   Unit of Work
│       ├── Identity/
│       │   ├── Authentication/        #   Login services
│       │   ├── Authorization/         #   Permission-based auth
│       │   ├── CurrentUser/           #   Current user context
│       │   ├── Invitations/           #   Invitation token services
│       │   ├── Providers/             #   Google auth provider
│       │   ├── Security/              #   Hashing, crypto helpers
│       │   ├── Tokens/                #   JWT generation & refresh
│       │   └── UserManagement/        #   User CRUD operations
│       ├── External/
│       │   ├── Email/                 #   MailKit SMTP service
│       │   ├── FileStorage/           #   Local file storage
│       │   ├── Cache/                 #   In-memory caching
│       │   ├── Client/               #   HTTP client services
│       │   ├── Jobs/                  #   Hangfire job definitions
│       │   └── Routing/              #   URL generation helpers
│       ├── BackgroundJobs/            #   Outbox message processor
│       ├── Notifications/             #   Email notification service
│       ├── Options/                   #   Strongly-typed config (Options pattern)
│       └── DependencyInjection/       #   Infrastructure DI registration
│
└── tests/
    └── Skill-Loop.UnitTests/          # 🧪 Unit Tests (incl. security regression suites)
```

---

## 🚀 Getting Started

### Prerequisites

| Tool | Version |
|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0+ |
| [SQL Server](https://www.microsoft.com/sql-server) | 2019+ (or LocalDB) |
| [Git](https://git-scm.com/) | Any recent version |

### 1. Clone the Repository

```bash
git clone https://github.com/your-username/Skill-Loop.git
cd Skill-Loop
```

### 2. Configure the Application

> ⚠️ **Secrets are never stored in `appsettings.json`.** That file is tracked in Git and ships
> with empty secret values. The application **refuses to start** if a required secret is
> missing, too short, or matches a value that was previously committed. See
> [`docs/SECURITY.md`](docs/SECURITY.md) for the full list of requirements.

#### Local development — user-secrets

> Do **not** run `dotnet user-secrets init`. `Skill-Loop.Api.csproj` already declares a
> `UserSecretsId`, so that command fails with *"UserSecretsId is already set"*.

```bash
cd src/Skill-Loop.Api

dotnet user-secrets set "Jwt:Key"                 "$(openssl rand -base64 48)"
dotnet user-secrets set "OtpSettings:HashingSecret" "$(openssl rand -base64 48)"
dotnet user-secrets set "MailSettings:Host"      "smtp.example.com"
dotnet user-secrets set "MailSettings:Username"  "your-username"
dotnet user-secrets set "MailSettings:Password"  "your-app-password"
dotnet user-secrets set "MailSettings:SenderEmail" "your-email@example.com"
```

`Jwt:Key` and `OtpSettings:HashingSecret` are the only two the application will refuse to
start without; the mail values only matter if you exercise the email endpoints.

Then start the API:

```powershell
# from the repository root
powershell -ExecutionPolicy Bypass -File docs\run-api-local.ps1
```

That script creates both databases (`Skill-Loop` and the separate `Skill-Loop-Hangfire`),
falls back to the in-memory cache because there is no Redis locally, points SQL at the
`localhost` instance, and validates your secrets before handing off to `dotnet run`. See
[`docs/TESTING_RUNBOOK.md`](docs/TESTING_RUNBOOK.md).

Seeding the first `SuperAdmin` is opt-in. Without these two values no admin account is
created — there is no default password anywhere in the codebase.

```bash
dotnet user-secrets set "Seed:SuperAdmin:Email"    "admin@your-domain.com"
dotnet user-secrets set "Seed:SuperAdmin:Password" "$(openssl rand -base64 24)Aa1!"
```

#### Production — environment variables

Use `__` as the section separator:

```bash
export Jwt__Key="..."
export OtpSettings__HashingSecret="..."
export MailSettings__Password="..."
export Seed__SuperAdmin__Email="..."
export Seed__SuperAdmin__Password="..."
```

#### Non-secret settings

`appsettings.json` holds only non-sensitive structure and carries inline comments for each
key. These are the values you normally edit:

```jsonc
{
  "ConnectionStrings": {
    "DefaultConnection":  "Server=...;Database=Skill-Loop;...",
    // Must be a SEPARATE database from the application database.
    "HangfireConnection": "Server=...;Database=Skill-Loop-Hangfire;..."
  },

  "CorsSettings": {
    // "*" is rejected at startup, as is any loopback or plain-HTTP origin
    // outside Development.
    "AllowedOrigins": ["https://app.your-domain.com"]
  },

  "Jwt": {
    "Issuer":         "https://api.your-domain.com",
    "Audience":       "https://api.your-domain.com",
    "ExpiryMinutes":  60   // must be > 0 and <= 60
  },

  "OtpSettings": {
    "CodeLength":      6,
    "Expiry":          "00:05:00",
    "ResendCooldown":  "00:02:00",  // must be >= 00:01:00
    "MaxAttempts":     3
  }
}
```

### 3. Apply Database Migrations

```bash
cd src/Skill-Loop.Api
dotnet ef database update --project ../Skill-Loop.Infrastructure
```

> The `HardenRefreshTokenStorage` migration changes how refresh tokens are stored and
> **deletes all existing refresh tokens**, so every user must sign in again. See
> [`docs/SECURITY.md` §3.3](docs/SECURITY.md#33-refresh-token-storage-and-rotation).

### 4. Run the Application

```bash
dotnet run --project src/Skill-Loop.Api
```

The API will be available at:
- **HTTPS**: `https://localhost:7271`
- **API Docs (Scalar)**: `https://localhost:7271/scalar/v1`

---

## 📡 API Endpoints

> ### 🛡️ Rate-limited endpoints
>
> Unauthenticated endpoints are throttled per **IP and target identity** — see
> [`docs/SECURITY.md` §3.4](docs/SECURITY.md#34-rate-limiting) for the full limit table.
> Exceeding a limit returns `429` with `Retry-After`.

### 🔐 Authentication (`/api/v1/auth`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/v1/auth/staff/login` | ❌ | Staff login (email + password) |
| `POST` | `/api/v1/auth/staff/login/google` | ❌ | Staff login via Google OAuth |
| `POST` | `/api/v1/auth/user/login` | ❌ | User login (email + password) |
| `POST` | `/api/v1/auth/user/login/google` | ❌ | User login via Google OAuth |
| `POST` | `/api/v1/auth/user/register` | ❌ | Register new user |
| `POST` | `/api/v1/auth/user/verify-email` | ❌ | Verify email with OTP |
| `POST` | `/api/v1/auth/user/resend-verification-code` | ❌ | Resend email verification code |
| `POST` | `/api/v1/auth/refresh-token` | ❌ | Refresh access token |
| `POST` | `/api/v1/auth/logout` | ✅ | Revoke refresh token |
| `POST` | `/api/v1/auth/password/forgot` | ❌ | Request password reset link |
| `POST` | `/api/v1/auth/password/reset` | ❌ | Reset password with token |

### 👤 Profile Management (`/api/v1/me`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/v1/me` | ✅ | Get current user profile |
| `PUT` | `/api/v1/me` | ✅ | Update profile info |
| `PATCH` | `/api/v1/me/picture` | ✅ | Update avatar |
| `POST` | `/api/v1/me/change-password` | ✅ | Change current password |

### 👥 User Management (`/api/v1/users`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/v1/users` | 🔒 Admin,SuperAdmin | List all users (paginated) |
| `PATCH` | `/api/v1/users/{userId}/activate` | 🔒 Admin,SuperAdmin | Activate a user |
| `PATCH` | `/api/v1/users/{userId}/deactivate` | 🔒 Admin,SuperAdmin | Deactivate a user (revokes all sessions) |
| `POST` | `/api/v1/users/{userId}/roles` | 🔒 Admin,SuperAdmin | Assign role to user |
| `DELETE` | `/api/v1/users/{userId}/roles/{roleName}` | 🔒 Admin,SuperAdmin | Remove role from user |

### 📩 Staff Invitations (`/api/v1/staff-invitations`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/v1/staff-invitations/send` | 🔒 Admin,SuperAdmin | Send invitation email |
| `GET` | `/api/v1/staff-invitations/validate/{token}` | ❌ | Validate invitation token |
| `POST` | `/api/v1/staff-invitations/accept` | ❌ | Accept with password |
| `POST` | `/api/v1/staff-invitations/accept-google` | ❌ | Accept with Google account |

### 🔑 Permission Management (`/api/v1/permission-management`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/v1/permission-management/permissions` | ✅ | Get all permissions |
| `GET` | `/api/v1/permission-management/roles` | ✅ | Get all roles with permissions |
| `GET` | `/api/v1/permission-management/roles/{roleId}` | ✅ | Get permissions for a role |
| `POST` | `/api/v1/permission-management/roles/{roleId}/permissions/{permissionId}/assign` | ✅ | Assign permission to role |
| `POST` | `/api/v1/permission-management/roles/{roleId}/permissions/{permissionId}/remove` | ✅ | Remove permission from role |
| `POST` | `/api/v1/permission-management/roles/{roleId}/permissions/update` | ✅ | Batch update role permissions |

> **Legend:** ❌ Public · ✅ Authenticated · 🔒 Admin Only

### 🎤 Instructor Profiles (`/api/instructor-profiles`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/instructor-profiles` | ❌ | List approved instructors (paginated, filterable) |
| `GET` | `/api/instructor-profiles/{userId}` | ❌ | Get instructor profile by user ID |
| `GET` | `/api/instructor-profiles/me` | ✅ | Get current user's instructor profile |
| `POST` | `/api/instructor-profiles/me` | ✅ | Create instructor profile for current user |
| `PUT` | `/api/instructor-profiles/me` | ✅ | Update instructor profile |
| `PATCH` | `/api/instructor-profiles/users/{userId}/approval-status` | 🔒 Admin | Approve/suspend an instructor |
| `POST` | `/api/instructor-profiles/{profileId}/availabilities` | ✅ | Add availability slot |
| `DELETE` | `/api/instructor-profiles/{profileId}/availabilities/{availabilityId}` | ✅ | Remove availability slot |
| `POST` | `/api/instructor-profiles/{profileId}/reviews` | ✅ | Add a review for an instructor |
| `PUT` | `/api/instructor-profiles/{profileId}/reviews/{reviewId}` | ✅ | Update a review |
| `DELETE` | `/api/instructor-profiles/{profileId}/reviews/{reviewId}` | ✅ | Remove a review |

### 💰 Wallets (`/api/wallets`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/wallets/me` | ✅ | Get current user's wallet balance |
| `GET` | `/api/wallets/me/transactions` | ✅ | Get transaction history (paginated) |

### 📚 Courses (`/api/v1/courses`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/v1/courses` | ❌ | List courses (paged, search, filter, sort) |
| `GET` | `/api/v1/courses/{id}` | ❌ | Get course details with syllabus |
| `POST` | `/api/v1/courses` | ✅ | Create a new course (draft) |
| `POST` | `/api/v1/courses/{id}/publish` | ✅ | Publish course to catalog |
| `POST` | `/api/v1/courses/{id}/sections/{sectionId}/lessons` | ✅ | Add lesson to section |
| `POST` | `/api/v1/courses/{id}/reviews` | ✅ | Add course review & rating |
| `POST` | `/api/v1/courses/{id}/bookmark` | ✅ | Toggle course bookmark |

### 📝 Enrollments (`/api/v1/enrollments`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/v1/enrollments/enroll` | ✅ | Enroll in course (atomic credit deduction) |
| `GET` | `/api/v1/enrollments/my-courses` | ✅ | Get enrolled courses with progress |
| `POST` | `/api/v1/enrollments/{courseId}/lessons/progress` | ✅ | Update lesson progress |

### 🗂️ Categories (`/api/v1/categories`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/v1/categories` | ❌ | List all categories with course counts |
| `POST` | `/api/v1/categories` | 🔒 Admin | Create category (with icon upload) |
| `PUT` | `/api/v1/categories/{id}` | 🔒 Admin | Update category |
| `DELETE` | `/api/v1/categories/{id}` | 🔒 Admin | Delete category |

### 🎓 Sessions (`/api/v1/sessions`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/v1/sessions` | ✅ | List sessions (filter by instructor, status, date, bookable) |
| `GET` | `/api/v1/sessions/me` | ✅ | Instructor's own sessions (upcoming/past) |
| `GET` | `/api/v1/sessions/{id}` | ✅ | Get session details |
| `POST` | `/api/v1/sessions` | ✅ | Create session (draft) |
| `PUT` | `/api/v1/sessions/{id}` | ✅ | Update session |
| `DELETE` | `/api/v1/sessions/{id}` | ✅ | Delete session |
| `PATCH` | `/api/v1/sessions/{id}/status` | ✅ | Change session status |

### 🎟️ Session Materials (`/api/sessions/{sessionId}/materials`)

> This controller is **not** versioned (`api/sessions`, not `api/v1/sessions`).

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/sessions/{sessionId}/materials` | ✅ | List session materials |
| `GET` | `/api/sessions/{sessionId}/materials/{materialId}/download` | ✅ | Download the material file (streamed from Google Drive) |
| `POST` | `/api/sessions/{sessionId}/materials` | ✅ | Upload session material |
| `DELETE` | `/api/sessions/{sessionId}/materials/{materialId}` | ✅ | Delete session material |
| `PUT` | `/api/sessions/{sessionId}/materials/reorder` | ✅ | Reorder materials |

### 🎟️ Bookings (`/api/v1/bookings`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/v1/bookings` | ✅ | Book a session (deducts credits) |
| `POST` | `/api/v1/bookings/{id}/cancel` | ✅ | Cancel booking (refunds credits) |
| `PATCH` | `/api/v1/bookings/{id}/status` | ✅ (Instructor) | Change booking status |
| `GET` | `/api/v1/bookings/me` | ✅ | Get my bookings as learner |
| `GET` | `/api/v1/bookings/session/{sessionId}` | ✅ | Get bookings for a session (instructor) |
| `GET` | `/api/v1/bookings/{id}` | ✅ | Get booking details |

### 💬 Chat (`/api/v1/chat`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/v1/chat/conversations` | ✅ | Start/get conversation |
| `GET` | `/api/v1/chat/conversations` | ✅ | List my conversations |
| `GET` | `/api/v1/chat/conversations/{id}/messages` | ✅ | Get conversation messages (paged) |
| `POST` | `/api/v1/chat/conversations/{id}/messages` | ✅ | Send message |
| `POST` | `/api/v1/chat/conversations/{id}/read` | ✅ | Mark messages as read |

> **Real-time:** SignalR Hub at `/hubs/chat` — `ReceiveMessage`, `MessagesRead` events

### 🛟 Support & FAQ (`/api/support`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/support/questions` | ❌ | Search published FAQ (paged, search, category) |
| `GET` | `/api/support/questions/{id}` | ❌ | Get a published FAQ question |
| `POST` | `/api/support/contact` | ✅ | Submit a support question (identity from JWT) |
| `GET` | `/api/support/questions/mine` | ✅ | My support questions + their answers |
| `POST` | `/api/support/questions/{id}/email-answer` | ✅ | Email a published answer to me |

### 🛟 Support Management (`/api/support/manage`) — 🔒 Admin,Staff

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/support/manage/questions` | 🔒 Admin,Staff | All questions (filter by published/answered) |
| `GET` | `/api/support/manage/questions/unanswered` | 🔒 Admin,Staff | Questions waiting for an answer |
| `GET` | `/api/support/manage/questions/{id}` | 🔒 Admin,Staff | Full details incl. asker email |
| `POST` | `/api/support/manage/questions` | 🔒 Admin,Staff | Create a FAQ question |
| `POST` | `/api/support/manage/questions/{id}/answer` | 🔒 Admin,Staff | Answer + email the answer |
| `PUT` | `/api/support/manage/questions/{id}` | 🔒 Admin,Staff | Update a question |
| `PATCH` | `/api/support/manage/questions/{id}/publication` | 🔒 Admin,Staff | Publish / unpublish |
| `DELETE` | `/api/support/manage/questions/{id}` | 🔒 Admin,Staff | Delete a question |

### 🔔 Notifications (`/api/v1/notifications`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/v1/notifications` | ✅ | My notifications (paged, newest first) |
| `GET` | `/api/v1/notifications/unread-count` | ✅ | Unread notification count |
| `POST` | `/api/v1/notifications/{notificationId}/read` | ✅ | Mark one as read |
| `POST` | `/api/v1/notifications/read-all` | ✅ | Mark all as read |

### ⚙️ Site Settings (`/api/SiteSettings`)

> This controller has no route attribute, so it inherits `BaseApiController`'s `api/[controller]` — hence the capitalized segment.

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/SiteSettings` | ❌ | Get current site settings |
| `PUT` | `/api/SiteSettings` | 🔒 SuperAdmin | Update site settings |

---

## 🔄 Request Pipeline

### HTTP middleware (`PipelineExtensions.UseApplicationPipeline`)

Order is significant:

```
Request
  │
  ├─ 1. CorrelationIdMiddleware     → Validated trace ID (≤64 chars, alphanumeric)
  ├─ 2. SecurityHeadersMiddleware   → nosniff, frame-deny, CSP, referrer-policy
  ├─ 3. RateLimitIdentityMiddleware → Buffers/rewinds body, extracts target identity
  ├─ 4. UseRateLimiter              → Global per-IP limit + per-endpoint policies
  ├─ 5. UseExceptionHandler         → 401 / 409 / ProblemDetails
  ├─ 6. UseHsts + UseHttpsRedirection
  ├─ 7. UseCors("AllowFrontend")    → Explicit origin allowlist, no wildcard
  ├─ 8. UseStaticFiles              → /uploads
  ├─ 9. UseOpenApiDocumentation
  ├─ 10. UseAuthentication
  ├─ 11. UseAuthorization
  ├─ 12. MapControllers
  └─ 13. MapChatHub                 → /hubs/chat
```

`RateLimitIdentityMiddleware` must run **before** `UseRateLimiter`: the limiter partitions
buckets on the identity it extracts. It buffers and rewinds the body so model binding is
unaffected.

### MediatR behaviors

```
Request
  │
  ├─ 1. LoggingBehavior              → Logs start/end of every request
  ├─ 2. PerformanceBehavior          → Alerts on slow requests
  ├─ 3. AuthorizationBehavior        → Checks user permissions
  │     CourseOwnershipBehavior
  │     SessionOwnershipBehavior
  ├─ 4. ValidationBehavior           → Runs FluentValidation rules
  ├─ 5. CachingBehavior              → Returns cached response (Queries)
  ├─ 6. CacheInvalidationBehavior    → Clears cache (Commands)
  ├─ 7. TransactionBehavior          → DB transaction for ICommand<TResponse>
  └─ 8. NonGenericCommandTransactionBehavior → DB transaction for ICommand
        │
        └─ Handler → Executes business logic
```

> Both transaction behaviors are required. `TransactionBehavior` is constrained to
> `ICommand<TResponse>`, so without the second one, non-returning commands such as
> `DeactivatePromoCode` and `AssignPermissionToRole` would run with no transaction at all.

---

## 🧪 Testing

```bash
dotnet test tests/Skill-Loop.UnitTests
```

The suite includes dedicated security regression coverage for refresh-token hashing and
rotation, session revocation on password reset, startup secret validation, and correlation-ID
input sanitisation. See [`docs/SECURITY.md` §6](docs/SECURITY.md#6-security-regression-tests)
for the mapping of suites to controls.

---

## 📚 Documentation

Deeper docs live in [`docs/`](docs):

| Doc | Covers |
|-----|--------|
| **[SECURITY.md](docs/SECURITY.md)** | **Secret rotation runbook, configuration requirements, and every security control** |
| [ARCHITECTURE_GUIDE.md](docs/ARCHITECTURE_GUIDE.md) | Layer-by-layer walkthrough: entities, enums, events, behaviors, DI, caching |
| [explanation.md](docs/explanation.md) | High-level code map and request flows |
| [BACKEND_GAP_ANALYSIS.md](docs/BACKEND_GAP_ANALYSIS.md) | Feature matrix, test coverage, and known gaps |
| [TESTING_RUNBOOK.md](docs/TESTING_RUNBOOK.md) | Running and writing tests |
| [Support_Subsystem.md](docs/Support_Subsystem.md) | FAQ + support requests: endpoints, caching, email, domain rules |
| [Chat_Subsystem.md](docs/Chat_Subsystem.md) | Real-time chat and SignalR |
| [Course_And_Enrollment_Subsystem.md](docs/Course_And_Enrollment_Subsystem.md) | Courses, sections, lessons, enrollment, progress |
| [PERMISSIONS_MATRIX.md](docs/PERMISSIONS_MATRIX.md) | Role-to-permission reference |

---

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## 📝 License

This project is licensed under the **MIT License**.

---

<p align="center">
  Built with ❤️ as a Graduation Project
</p>
