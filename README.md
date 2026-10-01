<![CDATA[<p align="center">
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
| 🎤 **Instructor Profiles** | Create/Update Profile · Approval Workflow · Availability Slots · Reviews & Ratings |
| 💰 **Wallets** | View Balance · Transaction History (Paginated) · Buy Credits · Earnings Summary · Optimistic Concurrency |
| 📚 **Courses & Enrollments** | Course CRUD & Publish · Sections & Lessons · Course Reviews · Bookmarks · Enrollment · Lesson Progress · Course Materials |
| 🎓 **Sessions & Materials** | Session CRUD · Status Management · File Upload/Download · Material Reorder · Session Reviews |
| 🎟️ **Bookings** | Direct Booking (Calendly-style) · Session Booking · Cancel & Refund · Status Management |
| 💬 **Real-time Chat** | 1-on-1 Conversations · SignalR WebSocket · Read Receipts · Message Notifications |
| 🛟 **Support & FAQ** | Public FAQ Search · Authenticated Contact Requests · Staff Answering & Publishing · Email Notifications |
| 🔔 **In-App Notifications** | Notification feed (paginated) · Unread count · Mark one/all as read |
| 📩 **Staff Invitations** | Admin sends invite via email → Staff accepts with password or Google account |
| 🔑 **Permission System** | Granular module-based permissions · Role–Permission assignment · Dynamic RBAC |
| 📱 **OTP Verification** | HMAC-hashed codes · Configurable expiry & cooldown · Max attempts lockout |
| 🎟️ **Promo Codes** | Create/list/deactivate promo codes · Public validation · Permission-gated management |
| 💳 **Credit Purchases** | Purchase credits with promo code support · Get purchase quotes |
| 📊 **Dashboards** | Admin / Instructor / Student dashboard summaries |
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

- **Domain** — Pure business entities (`Course`, `InstructorProfile`, `Session`, `UserWallet`, `StaffInvitation`, `OtpVerification`, `PromoCode`, `AuditLog`), base entity types (`AuditableEntity`, `SoftDeleteEntity`), domain events, enums, and the `Result<T>` pattern for error handling.
- **Application** — Commands & Queries (CQRS) via MediatR, FluentValidation, AutoMapper, and a rich pipeline of cross-cutting behaviors (Logging → Performance → Authorization → Validation → Caching → Cache Invalidation → Transaction).
- **Infrastructure** — EF Core with SQL Server, ASP.NET Core Identity, JWT token management, Google Auth, email (MailKit/SMTP), file storage (Local + Google Drive — switchable via config), Hangfire background jobs, Outbox pattern, and cache-aside caching (in-memory + Redis).
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

### File Storage
- **Local file system** — Default provider (`FileStorage:Provider = "Local"`)
- **Google Drive** — Optional provider via service-account key (`FileStorage:Provider = "GoogleDrive"`)

### API Documentation
- **Scalar** (OpenAPI/Swagger alternative)

---

## 📂 Project Structure

```
Skill-Loop/
├── Skill-Loop.slnx                    # Solution file
├── README.md
├── docker-compose.yml                 # API + SQL Server + Redis
├── .env.example                       # Template for Docker secrets
│
├── src/
│   ├── Skill-Loop.Api/                # 🌐 Presentation Layer
│   │   ├── Controllers/
│   │   │   ├── AuthController              # Login, Google login, register, refresh, logout
│   │   │   ├── OtpController               # OTP verify & resend
│   │   │   ├── PasswordsController         # Forgot & reset password
│   │   │   ├── ProfileController           # Current user profile CRUD
│   │   │   ├── UsersController             # Admin user management
│   │   │   ├── StaffInvitationsController  # Staff invitation flow
│   │   │   ├── PermissionManagementController  # RBAC management
│   │   │   ├── CoursesController           # Courses CRUD
│   │   │   ├── CourseSectionsController    # Course sections CRUD
│   │   │   ├── CourseLessonsController     # Course lessons CRUD
│   │   │   ├── CourseMaterialsController   # Course & lesson material uploads
│   │   │   ├── CourseReviewsController     # Course reviews CRUD
│   │   │   ├── CategoriesController        # Categories CRUD
│   │   │   ├── EnrollmentsController       # Course enrollment & progress
│   │   │   ├── SessionsController          # Sessions CRUD
│   │   │   ├── SessionMaterialsController  # Session file management
│   │   │   ├── SessionReviewsController    # Session reviews CRUD
│   │   │   ├── SessionBookingsController   # Session-scoped booking list
│   │   │   ├── BookingsController          # Booking management
│   │   │   ├── InstructorProfilesController    # Instructor profiles & approval
│   │   │   ├── InstructorAvailabilitiesController  # Availability slots
│   │   │   ├── InstructorReviewsController # Instructor reviews
│   │   │   ├── WalletsController           # Wallet balance, transactions, credits, earnings
│   │   │   ├── CreditPurchasesController   # Credit purchase & quotes
│   │   │   ├── PromoCodesController        # Promo code management
│   │   │   ├── ChatController              # Real-time chat
│   │   │   ├── SupportController           # Public FAQ + user contact requests
│   │   │   ├── SupportManagementController # Staff support queue
│   │   │   ├── NotificationsController     # In-app notifications
│   │   │   ├── DashboardsController        # Admin/Instructor/Student dashboards
│   │   │   ├── SiteSettingsController      # Site settings (SuperAdmin)
│   │   │   ├── AdminEmailsController       # Admin email testing & resend
│   │   │   ├── AuditLogsController         # System audit logs (SuperAdmin)
│   │   │   ├── DevController               # Dev-only helpers (404 outside Development)
│   │   │   └── TestFilesController         # File storage test endpoints
│   │   ├── Contracts/                  # Request/Response DTOs
│   │   ├── Middlewares/                # CorrelationId, SecurityHeaders, GlobalExceptionHandler
│   │   ├── Extensions/                # Pipeline, DI, CORS, RateLimiting, SecurityConfiguration
│   │   └── appsettings.json           # Configuration (tracked, contains NO secrets)
│   │
│   ├── Skill-Loop.Application/        # 📋 Application Layer
│   │   ├── Features/
│   │   │   ├── Accounts/              # Auth, Account Mgmt, Permissions, Invitations
│   │   │   ├── Categories/            # Categories Commands & Queries
│   │   │   ├── Courses/               # Course Commands, Queries & Events
│   │   │   ├── CreditPurchases/       # Credit purchase & quote
│   │   │   ├── Dashboards/            # Dashboard summaries
│   │   │   ├── Emails/                # Admin email commands
│   │   │   ├── Enrollments/           # Enrollment & Lesson Progress
│   │   │   ├── Instructors/           # Instructor Profile CRUD, Reviews, Availability
│   │   │   ├── Notifications/         # In-app notifications
│   │   │   ├── Promotions/            # Promo code management
│   │   │   ├── Sessions/              # Sessions & Materials Commands & Queries
│   │   │   ├── Bookings/              # Booking commands & queries
│   │   │   ├── Chat/                  # Chat Commands & Queries
│   │   │   ├── Support/               # FAQ & support requests
│   │   │   ├── SiteSettings/          # Site settings Commands/Queries
│   │   │   └── Wallets/               # Wallet Queries, DTOs & EventHandlers
│   │   ├── Common/
│   │   │   ├── Behaviors/             # MediatR pipeline behaviors
│   │   │   ├── Abstractions/          # Service interfaces
│   │   │   ├── Pagination/            # Paginated result helpers
│   │   │   ├── Validation/            # Shared validators
│   │   │   └── Errors/               # Error constants
│   │   └── DependencyInjection.cs     # Application DI registration
│   │
│   ├── Skill-Loop.Domain/            # 🏛️ Domain Layer
│   │   ├── Entities/
│   │   │   ├── Booking/               # Booking entity
│   │   │   ├── Chat/                  # Conversation, ChatMessage
│   │   │   ├── Courses/               # Course aggregate, Value Objects, Events
│   │   │   ├── Emails/                # Email tracking entity
│   │   │   ├── Enrollments/           # Enrollment, LessonProgress, Events
│   │   │   ├── Instructors/           # InstructorProfile, InstructorReview
│   │   │   ├── Invitation/            # StaffInvitation + domain events
│   │   │   ├── OtpVerification/       # OTP entity
│   │   │   ├── Promotions/            # PromoCode entity
│   │   │   ├── Session/               # Session, SessionMaterial, Events
│   │   │   ├── SiteSettings/          # SiteSettings entity
│   │   │   ├── Support/               # SupportQuestion entity
│   │   │   ├── System/                # AuditLog entity
│   │   │   ├── Notifications/         # Notification entity
│   │   │   └── Wallets/               # UserWallet, WalletTransaction, Events
│   │   ├── Common/
│   │   │   ├── Entities/              # BaseEntity, AuditableEntity, SoftDeleteEntity
│   │   │   ├── Events/               # Domain event base types
│   │   │   └── Results/              # Result<T>, Error, ErrorType
│   │   ├── Enums/                     # OtpPurpose, SessionStatus, BookingStatus, etc.
│   │   └── Constants/                 # Roles, Permissions
│   │
│   └── Skill-Loop.Infrastructure/     # 🔧 Infrastructure Layer
│       ├── Persistence/
│       │   ├── Data/                  # AppDbContext, migrations factory
│       │   ├── IdentityModels/        # ApplicationUser, ApplicationRole, RefreshToken
│       │   ├── Configurations/        # EF Core entity configurations
│       │   ├── Interceptors/          # SaveChanges interceptors
│       │   ├── Outbox/                # Outbox message pattern
│       │   ├── Seed/                  # Database seeder
│       │   └── Transaction/           # Unit of Work
│       ├── Identity/
│       │   ├── Authentication/        # Login services
│       │   ├── Authorization/         # Permission-based auth
│       │   ├── CurrentUser/           # Current user context
│       │   ├── Invitations/           # Invitation token services
│       │   ├── Providers/             # Google auth provider
│       │   ├── Security/              # Hashing, crypto helpers
│       │   ├── Tokens/                # JWT generation & refresh
│       │   └── UserManagement/        # User CRUD operations
│       ├── External/
│       │   ├── Email/                 # MailKit SMTP service
│       │   ├── FileStorage/           # Local + Google Drive file storage
│       │   ├── Storage/               # Google Drive content storage
│       │   ├── Cache/                 # In-memory caching
│       │   ├── Client/               # HTTP client services
│       │   ├── Jobs/                  # Hangfire job definitions
│       │   └── Routing/              # URL generation helpers
│       ├── BackgroundJobs/            # Outbox processor + Drive quota refresh
│       ├── Notifications/             # Email notification service
│       ├── Options/                   # Strongly-typed config (Options pattern)
│       └── DependencyInjection/       # Infrastructure DI registration
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
git clone https://github.com/aymansalama48/Skill-Loop-Backend.git
cd Skill-Loop-Backend
```

### 2. Configure the Application

> ⚠️ **Secrets are never stored in `appsettings.json`.** That file is tracked in Git and ships
> with empty/placeholder secret values. The application **refuses to start** if a required secret is
> missing, too short, or matches a previously committed value. See
> [`docs/SECURITY.md`](docs/SECURITY.md) for the full list of requirements.

#### Local development — user-secrets

> Do **not** run `dotnet user-secrets init`. `Skill-Loop.Api.csproj` already declares a
> `UserSecretsId`, so that command fails with *"UserSecretsId is already set"*.

```bash
cd src/Skill-Loop.Api

dotnet user-secrets set "Jwt:Key"                  "$(openssl rand -base64 48)"
dotnet user-secrets set "OtpSettings:HashingSecret" "$(openssl rand -base64 48)"
dotnet user-secrets set "MailSettings:Host"         "smtp.example.com"
dotnet user-secrets set "MailSettings:Username"     "your-username"
dotnet user-secrets set "MailSettings:Password"     "your-app-password"
dotnet user-secrets set "MailSettings:SenderEmail"  "your-email@example.com"
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

### 3. Apply Database Migrations

```bash
cd src/Skill-Loop.Api
dotnet ef database update --project ../Skill-Loop.Infrastructure
```

### 4. Run the Application

```bash
dotnet run --project src/Skill-Loop.Api
```

The API will be available at:
- **HTTPS**: `https://localhost:7271`
- **API Docs (Scalar)**: `https://localhost:7271/scalar/v1`

---

## ⚙️ Configuration

All configuration keys used by the application. Secrets **must never** be committed to `appsettings.json`.

| Key | Description | Example / Placeholder | Required |
|-----|-------------|----------------------|----------|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string for app DB | `Server=localhost;Database=Skill-Loop;...` | ✅ Yes |
| `ConnectionStrings:HangfireConnection` | SQL Server for Hangfire (must be separate DB) | `Server=localhost;Database=Skill-Loop-Hangfire;...` | ✅ Yes |
| `Jwt:Key` | HMAC-SHA256 signing key (min 32 bytes) | `(openssl rand -base64 48)` | ✅ Yes 🔑 |
| `Jwt:Issuer` | JWT token issuer URL | `https://localhost:7271` | ✅ Yes |
| `Jwt:Audience` | JWT token audience URL | `https://localhost:7271` | ✅ Yes |
| `Jwt:ExpiryMinutes` | Access token lifetime (1–60) | `60` | ✅ Yes |
| `OtpSettings:HashingSecret` | HMAC key for OTP hashing (min 32 chars) | `(openssl rand -base64 48)` | ✅ Yes 🔑 |
| `OtpSettings:CodeLength` | OTP code digit count | `6` | Optional |
| `OtpSettings:Expiry` | OTP validity duration | `00:05:00` | Optional |
| `OtpSettings:ResendCooldown` | Min wait between resends (≥ 00:01:00) | `00:02:00` | Optional |
| `OtpSettings:MaxAttempts` | Max verification attempts | `3` | Optional |
| `MailSettings:SenderEmail` | SMTP from address | `noreply@example.com` | For email |
| `MailSettings:SenderName` | Display name for emails | `SkillLoop` | For email |
| `MailSettings:Host` | SMTP server hostname | `smtp.example.com` | For email |
| `MailSettings:Port` | SMTP port | `587` | For email |
| `MailSettings:Username` | SMTP username | `your-username` | For email 🔑 |
| `MailSettings:Password` | SMTP password / app-password | *(user-secrets only)* | For email 🔑 |
| `MailSettings:EnableSsl` | Use TLS/SSL for SMTP | `true` | For email |
| `FileStorage:Provider` | Storage backend: `Local` or `GoogleDrive` | `Local` | Optional |
| `FileStorage:RootFolder` | Local uploads directory name | `UploadedFiles` | Optional |
| `FileStorage:MaxFileSizeInMB` | Max upload size in MB | `500` | Optional |
| `GoogleDrive:ServiceAccountFilePath` | Path to Google service-account key JSON | `google-drive-key.json` | If GoogleDrive |
| `GoogleDrive:RootFolderId` | Root folder ID in Google Drive | *(your folder ID)* | If GoogleDrive 🔑 |
| `GoogleAuth:ClientId` | Google OAuth Client ID | *(your client ID)* | For Google login |
| `CorsSettings:AllowedOrigins` | Allowed CORS origins (array, no wildcards) | `["https://app.example.com"]` | ✅ Yes |
| `Redis:ConnectionString` | Redis connection string | `localhost:6379` | Optional |
| `Redis:InstanceName` | Redis key prefix | `SkillLoop:` | Optional |
| `BaseUrl:Backend` | API base URL for link generation | `https://localhost:7271` | ✅ Yes |
| `BaseUrl:Frontend` | Frontend URL for email links | `http://localhost:5173` | ✅ Yes |
| `Seed:SuperAdmin:Email` | Bootstrap admin email | `admin@example.com` | Optional 🔑 |
| `Seed:SuperAdmin:Password` | Bootstrap admin password | *(user-secrets only)* | Optional 🔑 |

> 🔑 = Secret — supply via user-secrets (dev) or environment variable (prod). Never commit.

### User Secrets (Local Development)

.NET User Secrets store sensitive values outside the project tree. They are never committed to Git.

```bash
cd src/Skill-Loop.Api
dotnet user-secrets set "Jwt:Key" "your-secret-value"
```

Secrets are stored in `%APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\secrets.json`.

### Environment Variables (Production / Docker)

Use `__` (double underscore) as the section separator:

```bash
export ConnectionStrings__DefaultConnection="Server=...;Password=...;"
export Jwt__Key="..."
export OtpSettings__HashingSecret="..."
export MailSettings__Password="..."
export Seed__SuperAdmin__Email="admin@example.com"
export Seed__SuperAdmin__Password="..."
```

---

## 🐳 Run with Docker

The project ships with a `Dockerfile`, `docker-compose.yml`, and `.env.example`.

### 1. Copy and fill the environment file

```bash
cp .env.example .env
# Edit .env with your actual values
```

### 2. Start the stack

```bash
docker compose up --build
```

This starts:
- **SQL Server 2022** — port `1433`, data persisted in a named volume
- **Redis 7** — port `6379`, for distributed caching
- **Skill Loop API** — port `8080`, waits for SQL & Redis health checks

### 3. Access the API

- **API**: `http://localhost:8080`
- **Scalar Docs**: `http://localhost:8080/scalar/v1`

### Services & Volumes

| Service | Image | Port | Volume |
|---------|-------|------|--------|
| `sqlserver` | `mcr.microsoft.com/mssql/server:2022-latest` | 1433 | `sql_data` |
| `redis` | `redis:7` | 6379 | — |
| `skill-loop.api` | Built from `src/Skill-Loop.Api/Dockerfile` | 8080 | `uploaded_files` |

---

## 📡 API Endpoints

> ### 🛡️ Rate-limited endpoints
>
> Unauthenticated endpoints are throttled per **IP and target identity** — see
> [`docs/SECURITY.md`](docs/SECURITY.md) for the full limit table.
> Exceeding a limit returns `429` with `Retry-After`.

**Legend:** ❌ Public · ✅ Authenticated · 🔒 Role-restricted

### 🔐 Authentication (`/api/v1/auth`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/v1/auth/staff/login` | ❌ | Staff login (email + password) |
| `POST` | `/api/v1/auth/staff/login/google` | ❌ | Staff login via Google OAuth |
| `POST` | `/api/v1/auth/user/login` | ❌ | User login (email + password) |
| `POST` | `/api/v1/auth/user/login/google` | ❌ | User login via Google OAuth |
| `POST` | `/api/v1/auth/user/register` | ❌ | Register new user |
| `POST` | `/api/v1/auth/refresh-token` | ❌ | Refresh access token |
| `POST` | `/api/v1/auth/logout` | ✅ | Revoke refresh token |

### 📱 OTP Verification (`/api/v1/auth/otp`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/v1/auth/otp/verify` | ❌ | Verify OTP code |
| `POST` | `/api/v1/auth/otp/resend` | ❌ | Resend OTP code |

### 🔑 Password Management (`/api/v1/auth/password`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/v1/auth/password/forgot` | ❌ | Request password reset link |
| `POST` | `/api/v1/auth/password/reset` | ❌ | Reset password with token |

### 👤 Profile (`/api/v1/profile`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/v1/profile` | ✅ | Get current user profile |
| `PUT` | `/api/v1/profile` | ✅ | Update profile info |
| `PATCH` | `/api/v1/profile/picture` | ✅ | Update avatar |
| `POST` | `/api/v1/profile/change-password` | ✅ | Change current password |

### 👥 User Management (`/api/v1/users`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/v1/users` | ✅ (Permission) | List all users (paginated) |
| `PATCH` | `/api/v1/users/{userId}/activate` | ✅ (Permission) | Activate a user |
| `PATCH` | `/api/v1/users/{userId}/deactivate` | ✅ (Permission) | Deactivate a user (revokes all sessions) |
| `POST` | `/api/v1/users/{userId}/roles` | ✅ (Permission) | Assign role to user |
| `DELETE` | `/api/v1/users/{userId}/roles/{roleName}` | ✅ (Permission) | Remove role from user |

### 📩 Staff Invitations (`/api/v1/staffinvitations`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/v1/staffinvitations/send` | ✅ (Permission) | Send invitation email |
| `GET` | `/api/v1/staffinvitations/validate/{token}` | ❌ | Validate invitation token |
| `POST` | `/api/v1/staffinvitations/accept` | ❌ | Accept with password |
| `POST` | `/api/v1/staffinvitations/accept-google` | ❌ | Accept with Google account |

### 🔑 Permission Management (`/api/v1/permissionmanagement`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/v1/permissionmanagement/permissions` | ✅ | Get all permissions |
| `GET` | `/api/v1/permissionmanagement/roles` | ✅ | Get all roles with permissions |
| `GET` | `/api/v1/permissionmanagement/roles/{roleId}` | ✅ | Get permissions for a role |
| `POST` | `/api/v1/permissionmanagement/roles/{roleId}/permissions/{permissionId}/assign` | ✅ | Assign permission to role |
| `POST` | `/api/v1/permissionmanagement/roles/{roleId}/permissions/{permissionId}/remove` | ✅ | Remove permission from role |
| `POST` | `/api/v1/permissionmanagement/roles/{roleId}/permissions/update` | ✅ | Batch update role permissions |

### 📚 Courses (`/api/v1/courses`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/v1/courses` | ❌ | List published courses (paged, search, filter, sort) |
| `GET` | `/api/v1/courses/{courseId}` | ❌ | Get course details with syllabus |
| `GET` | `/api/v1/courses/drafts` | ✅ | Get my draft courses |
| `GET` | `/api/v1/courses/me` | ✅ | Get my published courses |
| `GET` | `/api/v1/courses/bookmarks` | ✅ | Get my bookmarked courses |
| `POST` | `/api/v1/courses` | ✅ | Create a new course (draft, multipart) |
| `PUT` | `/api/v1/courses/{courseId}` | ✅ | Update course details |
| `POST` | `/api/v1/courses/{courseId}/publish` | ✅ | Publish course to catalog |
| `POST` | `/api/v1/courses/{courseId}/bookmark` | ✅ | Toggle course bookmark |
| `POST` | `/api/v1/courses/{courseId}/archive` | ✅ | Archive course |
| `DELETE` | `/api/v1/courses/{courseId}` | ✅ | Delete course |

### 📖 Course Sections (`/api/v1/courses/{courseId}/sections`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/v1/courses/{courseId}/sections` | ✅ | Add section to course |
| `PUT` | `/api/v1/courses/{courseId}/sections/{sectionId}` | ✅ | Update section |
| `DELETE` | `/api/v1/courses/{courseId}/sections/{sectionId}` | ✅ | Remove section |
| `PUT` | `/api/v1/courses/{courseId}/sections/reorder` | ✅ | Reorder sections |

### 📝 Course Lessons (`/api/v1/courses/{courseId}/sections/{sectionId}/lessons`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `.../lessons` | ✅ | Add lesson (multipart with video) |
| `PUT` | `.../lessons/{lessonId}` | ✅ | Update lesson (multipart) |
| `DELETE` | `.../lessons/{lessonId}` | ✅ | Remove lesson |
| `PUT` | `.../lessons/reorder` | ✅ | Reorder lessons |

### 📎 Course Materials (`/api/v1/courses/{courseId}`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `.../materials` | ✅ | Upload course-level material |
| `DELETE` | `.../materials/{materialId}` | ✅ | Remove course material |
| `POST` | `.../sections/{sectionId}/lessons/{lessonId}/materials` | ✅ | Upload lesson material |
| `DELETE` | `.../sections/{sectionId}/lessons/{lessonId}/materials/{materialId}` | ✅ | Remove lesson material |

### ⭐ Course Reviews (`/api/v1/courses/{courseId}/reviews`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/v1/courses/{courseId}/reviews` | ✅ | Add course review |
| `GET` | `/api/v1/courses/{courseId}/reviews` | ❌ | List course reviews (paged) |
| `PUT` | `/api/v1/courses/{courseId}/reviews` | ✅ | Update my review |
| `DELETE` | `/api/v1/courses/{courseId}/reviews` | ✅ | Delete my review |

### 🗂️ Categories (`/api/v1/categories`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/v1/categories` | ❌ | List all categories |
| `POST` | `/api/v1/categories` | ✅ | Create category (multipart) |
| `PUT` | `/api/v1/categories/{id}` | ✅ | Update category (multipart) |
| `DELETE` | `/api/v1/categories/{id}` | ✅ | Delete category |

### 📝 Enrollments (`/api/v1/enrollments`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/v1/enrollments/enroll` | ✅ | Enroll in course (atomic credit deduction) |
| `GET` | `/api/v1/enrollments/my-courses` | ✅ | Get enrolled courses with progress |
| `POST` | `/api/v1/enrollments/{courseId}/lessons/progress` | ✅ | Update lesson progress |
| `POST` | `/api/v1/enrollments/{courseId}/lessons/{lessonId}/complete` | ✅ | Mark lesson as complete |

### 🎓 Sessions (`/api/v1/sessions`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/v1/sessions` | ✅ | List sessions (filter by instructor, status, date, bookable) |
| `GET` | `/api/v1/sessions/me` | ✅ | Instructor's own sessions (upcoming/past) |
| `GET` | `/api/v1/sessions/{id}` | ✅ | Get session details |
| `POST` | `/api/v1/sessions` | ✅ | Create session |
| `PUT` | `/api/v1/sessions/{id}` | ✅ | Update session |
| `DELETE` | `/api/v1/sessions/{id}` | ✅ | Delete session |
| `PATCH` | `/api/v1/sessions/{id}/status` | ✅ | Change session status |

### 📁 Session Materials (`/api/v1/sessions`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/v1/sessions/{sessionId}/materials` | ✅ | List session materials |
| `GET` | `/api/v1/sessions/{sessionId}/materials/{materialId}/download` | ✅ | Download material file |
| `POST` | `/api/v1/sessions/{sessionId}/materials` | ✅ | Upload session material |
| `DELETE` | `/api/v1/sessions/{sessionId}/materials/{materialId}` | ✅ | Delete session material |
| `PUT` | `/api/v1/sessions/{sessionId}/materials/reorder` | ✅ | Reorder materials |

### ⭐ Session Reviews (`/api/v1/sessions/{sessionId}/reviews`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/v1/sessions/{sessionId}/reviews` | ✅ | Add session review |
| `GET` | `/api/v1/sessions/{sessionId}/reviews` | ❌ | List session reviews (paged) |
| `PUT` | `/api/v1/sessions/{sessionId}/reviews` | ✅ | Update my review |
| `DELETE` | `/api/v1/sessions/{sessionId}/reviews` | ✅ | Delete my review |

### 📋 Session Bookings (`/api/v1/sessions/{sessionId}/bookings`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/v1/sessions/{sessionId}/bookings` | ✅ | List bookings for a session |

### 🎟️ Bookings (`/api/v1/bookings`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/v1/bookings` | ✅ | Book a session (deducts credits) |
| `POST` | `/api/v1/bookings/direct` | ✅ | Direct booking (Calendly-style) |
| `POST` | `/api/v1/bookings/{id}/cancel` | ✅ | Cancel booking (refunds credits) |
| `PATCH` | `/api/v1/bookings/{id}/status` | ✅ | Change booking status (Instructor) |
| `GET` | `/api/v1/bookings/me` | ✅ | Get my bookings as learner |
| `GET` | `/api/v1/bookings/{id}` | ✅ | Get booking details |

### 🎤 Instructor Profiles (`/api/v1/instructor-profiles`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/v1/instructor-profiles` | ❌ | List approved instructors (paginated, filterable) |
| `GET` | `/api/v1/instructor-profiles/{userId}` | ❌ | Get instructor profile by user ID |
| `GET` | `/api/v1/instructor-profiles/me` | ✅ | Get current user's instructor profile |
| `POST` | `/api/v1/instructor-profiles/me` | ✅ | Create instructor profile |
| `PUT` | `/api/v1/instructor-profiles/me` | ✅ | Update instructor profile |
| `PATCH` | `/api/v1/instructor-profiles/{userId}/approval-status` | ✅ | Approve/suspend instructor |

### 📅 Instructor Availabilities (`/api/v1/instructor-profiles/{profileId}/availabilities`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `.../availabilities` | ✅ | Add availability slot |
| `DELETE` | `.../availabilities/{availabilityId}` | ✅ | Remove availability slot |

### ⭐ Instructor Reviews (`/api/v1/instructor-profiles/{profileId}/reviews`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `.../reviews` | ✅ | Add review for instructor |
| `PUT` | `.../reviews/{reviewId}` | ✅ | Update review |
| `DELETE` | `.../reviews/{reviewId}` | ✅ | Remove review |

### 💰 Wallets (`/api/v1/wallets`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/v1/wallets/me` | ✅ | Get wallet balance |
| `GET` | `/api/v1/wallets/me/transactions` | ✅ | Transaction history (paginated) |
| `POST` | `/api/v1/wallets/buy-credits` | ✅ | Buy credits (with optional promo code) |
| `GET` | `/api/v1/wallets/me/earnings` | ✅ | Earnings summary (filter by year/month) |

### 💳 Credit Purchases (`/api/v1/credit-purchases`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/v1/credit-purchases/quote` | ✅ | Get purchase quote (with promo code) |
| `POST` | `/api/v1/credit-purchases` | ✅ | Purchase credits |

### 🎟️ Promo Codes (`/api/v1/promo-codes`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/v1/promo-codes` | ✅ (Permission) | Create promo code |
| `GET` | `/api/v1/promo-codes` | ✅ (Permission) | List promo codes (paged) |
| `POST` | `/api/v1/promo-codes/{promoCodeId}/deactivate` | ✅ (Permission) | Deactivate promo code |
| `GET` | `/api/v1/promo-codes/{code}/validate` | ❌ | Validate promo code |

### 💬 Chat (`/api/v1/chat`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/v1/chat/conversations` | ✅ | Start/get conversation |
| `GET` | `/api/v1/chat/conversations` | ✅ | List my conversations |
| `GET` | `/api/v1/chat/conversations/{id}/messages` | ✅ | Get messages (paged) |
| `POST` | `/api/v1/chat/conversations/{id}/messages` | ✅ | Send message |
| `POST` | `/api/v1/chat/conversations/{id}/read` | ✅ | Mark messages as read |

> **Real-time:** SignalR Hub at `/hubs/chat` — `ReceiveMessage`, `MessagesRead` events

### 🛟 Support & FAQ (`/api/support`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/support/questions` | ❌ | Search published FAQ |
| `GET` | `/api/support/questions/{id}` | ❌ | Get FAQ question |
| `POST` | `/api/support/contact` | ✅ | Submit support question |
| `GET` | `/api/support/questions/mine` | ✅ | My questions + answers |
| `POST` | `/api/support/questions/{id}/email-answer` | ✅ | Email answer to me |

### 🛟 Support Management (`/api/support/manage`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/support/manage/questions` | ✅ | All questions (filterable) |
| `GET` | `/api/support/manage/questions/unanswered` | ✅ | Unanswered questions |
| `GET` | `/api/support/manage/questions/{id}` | ✅ | Full details |
| `POST` | `/api/support/manage/questions` | ✅ | Create FAQ question |
| `POST` | `/api/support/manage/questions/{id}/answer` | ✅ | Answer + email |
| `PUT` | `/api/support/manage/questions/{id}` | ✅ | Update question |
| `PATCH` | `/api/support/manage/questions/{id}/publication` | ✅ | Publish / unpublish |
| `DELETE` | `/api/support/manage/questions/{id}` | ✅ | Delete question |

### 🔔 Notifications (`/api/v1/notifications`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/v1/notifications` | ✅ | My notifications (paged) |
| `GET` | `/api/v1/notifications/unread-count` | ✅ | Unread count |
| `POST` | `/api/v1/notifications/{notificationId}/read` | ✅ | Mark one as read |
| `POST` | `/api/v1/notifications/read-all` | ✅ | Mark all as read |

### 📊 Dashboards (`/api/v1/dashboards`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/v1/dashboards/admin` | ❌ | Admin dashboard summary |
| `GET` | `/api/v1/dashboards/instructor` | ❌ | Instructor dashboard summary |
| `GET` | `/api/v1/dashboards/student` | ❌ | Student dashboard summary |

### ⚙️ Site Settings (`/api/v1/sitesettings`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/v1/sitesettings` | ❌ | Get current site settings |
| `PUT` | `/api/v1/sitesettings` | ✅ (Permission) | Update site settings |

### 📧 Admin Emails (`/api/v1/Admin/Emails`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/v1/Admin/Emails/test` | ✅ (Permission) | Send test email |
| `POST` | `/api/v1/Admin/Emails/{id}/resend` | ✅ (Permission) | Resend email |

### 📋 Audit Logs (`/api/v1/system/audit-logs`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/v1/system/audit-logs` | ✅ (Permission) | Get audit logs (paginated) |

### 🧪 Dev Helpers (`/api/v1/dev`) — Development Only

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/v1/dev/quick-login` | ❌ | Quick-login as admin (404 outside Development) |
| `GET` | `/api/v1/dev/users` | ❌ | List dev users (404 outside Development) |

### 🧪 Test Files (`/api/test/files`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/test/files/upload` | ✅ | Test file upload |
| `GET` | `/api/test/files/exists` | ✅ | Check file exists |
| `DELETE` | `/api/test/files/delete` | ✅ | Delete test file |

---

## 🔄 Request Pipeline

### HTTP middleware

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

---

## 🧪 Testing

```bash
dotnet test tests/Skill-Loop.UnitTests
```

The suite includes dedicated security regression coverage for refresh-token hashing and
rotation, session revocation on password reset, startup secret validation, and correlation-ID
input sanitisation. See [`docs/SECURITY.md`](docs/SECURITY.md) for details.

---

## 📚 Documentation

Deeper docs live in [`docs/`](docs):

| Doc | Covers |
|-----|--------|
| **[SECURITY.md](docs/SECURITY.md)** | Secret rotation runbook, configuration requirements, and every security control |
| [ARCHITECTURE_GUIDE.md](docs/ARCHITECTURE_GUIDE.md) | Layer-by-layer walkthrough: entities, enums, events, behaviors, DI, caching |
| [TESTING_RUNBOOK.md](docs/TESTING_RUNBOOK.md) | Running and writing tests |
| [PERMISSIONS_MATRIX.md](docs/PERMISSIONS_MATRIX.md) | Role-to-permission reference |
| [Chat_Subsystem.md](docs/Chat_Subsystem.md) | Real-time chat and SignalR |
| [Course_And_Enrollment_Subsystem.md](docs/Course_And_Enrollment_Subsystem.md) | Courses, sections, lessons, enrollment, progress |
| [Support_Subsystem.md](docs/Support_Subsystem.md) | FAQ + support requests: endpoints, caching, email, domain rules |

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
]]>
