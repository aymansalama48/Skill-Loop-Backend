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
| 🎤 **Instructor Profiles** | Create/Update Profile · Approval Workflow · Reviews & Ratings · Instructor Stats (Sessions Completed, Credits Earned) |
| 💰 **Wallets** | View Balance · Transaction History (Paginated) · Auto-Credit on Course Enrollment · Optimistic Concurrency |
| 📚 **Courses & Enrollments** | Course CRUD · Sections & Lessons · Course Reviews · Bookmarks · Enrollment · Lesson Progress Tracking |
| 🎓 **Sessions & Materials** | Session CRUD · Status Management · File Upload/Download (Google Drive) · Material Reordering |
| 💬 **Real-time Chat** | 1-on-1 Conversations · SignalR WebSocket · Read Receipts · Message Notifications |
| 📩 **Staff Invitations** | Admin sends invite via email → Staff accepts with password or Google account |
| 🔑 **Permission System** | Granular module-based permissions · Role–Permission assignment · Dynamic RBAC |
| 📱 **OTP Verification** | HMAC-hashed codes · Configurable expiry & cooldown · Max attempts lockout |
| 🎟️ **Live Session Booking** | Book sessions · Cancel with refund · Instructor status management · Credit payment |
| 📊 **Observability** | Structured logging (Serilog) · Correlation IDs · Performance tracking |
| ⚙️ **Background Jobs** | Outbox pattern with Hangfire for reliable domain event processing |

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
- **Infrastructure** — EF Core with SQL Server, ASP.NET Core Identity, JWT token management, Google Auth, email (MailKit/SMTP), file storage (local + Google Drive), Hangfire background jobs, Outbox pattern, and in-memory caching.
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
- **JWT Bearer Tokens** (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- **Google OAuth** (`Google.Apis.Auth`)
- **OTP with HMAC Hashing**

### Infrastructure
- **Hangfire** — Background job processing & outbox consumer
- **MailKit** — SMTP email delivery
- **Serilog** — Structured logging (Console + File sinks + enrichers)
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
│   │   │   ├── AccountsController     #     Auth, profile, user management
│   │   │   ├── StaffInvitationsController  # Invitation flow
│   │   │   ├── PermissionManagementController  # RBAC management
│   │   │   ├── CoursesController       #     Courses CRUD
│   │   │   ├── EnrollmentsController   #     Course enrollment & progress
│   │   │   ├── SessionsController      #     Sessions CRUD
│   │   │   ├── SessionMaterialsController  # Session file management
│   │   │   ├── InstructorProfilesController  # Instructor profiles & reviews
│   │   │   ├── WalletsController       #     Wallet balance & transactions
│   │   │   ├── ChatController          #     Real-time chat
│   │   │   └── BookingsController      #     Live session bookings
│   │   ├── Contracts/                  #   Request/Response DTOs
│   │   ├── Middlewares/                #   CorrelationId, GlobalExceptionHandler
│   │   ├── Extensions/                #   Pipeline & DI extensions
│   │   └── appsettings.json           #   Configuration
│   │
│   ├── Skill-Loop.Application/        # 📋 Application Layer
│   │   ├── Features/
│   │   │   ├── Accounts/              #   Auth, Account Mgmt, Permissions, Invitations
│   │   │   ├── Categories/            #   Categories Commands & Queries
│   │   │   ├── Courses/               #   Course Commands, Queries & Events
│   │   │   ├── Enrollments/           #   Enrollment & Lesson Progress
│   │   │   ├── Instructors/           #   Instructor Profile CRUD, Reviews, EventHandlers
│   │   │   ├── Wallets/               #   Wallet Queries, DTOs & EventHandlers
│   │   │   ├── Sessions/              #   Sessions & Materials Commands & Queries
│   │   │   ├── Chat/                  #   Chat Commands & Queries
│   │   │   ├── Otps/                  #   OTP verification logic
│   │   │   └── Bookings/              #   Live session booking commands & queries
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
    └── Skill-Loop.UnitTests/          # 🧪 Unit Tests
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

Update `src/Skill-Loop.Api/appsettings.json` with your settings:

```jsonc
{
  // Database connection
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=SkillLoop;Trusted_Connection=True;TrustServerCertificate=True;"
  },

  // JWT settings
  "Jwt": {
    "Key": "your-secure-secret-key-at-least-32-characters",
    "Issuer": "https://localhost:7271",
    "Audience": "https://localhost:7271",
    "ExpiryMinutes": 60
  },

  // Email (SMTP)
  "MailSettings": {
    "SenderEmail": "your-email@example.com",
    "Host": "smtp.example.com",
    "Port": 587,
    "Username": "your-username",
    "Password": "your-app-password",
    "EnableSsl": true
  },

  // Google OAuth (optional)
  "GoogleAuth": {
    "ClientId": "your-google-client-id"
  }
}
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

## 📡 API Endpoints

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
| `GET` | `/api/v1/users` | 🔒 Admin | List all users (paginated) |
| `PATCH` | `/api/v1/users/{userId}/activate` | 🔒 Admin | Activate a user |
| `PATCH` | `/api/v1/users/{userId}/deactivate` | 🔒 Admin | Deactivate a user |
| `POST` | `/api/v1/users/{userId}/roles` | 🔒 Admin | Assign role to user |
| `DELETE` | `/api/v1/users/{userId}/roles/{roleName}` | 🔒 Admin | Remove role from user |

### 📩 Staff Invitations (`/api/v1/staff-invitations`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/v1/staff-invitations/send` | 🔒 Admin | Send invitation email |
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

### 🎟️ Session Materials (`/api/v1/sessions/{sessionId}/materials`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/v1/sessions/{sessionId}/materials` | ✅ | List session materials |
| `GET` | `/api/v1/sessions/{sessionId}/materials/{materialId}/download` | ✅ | Get download info for material |
| `POST` | `/api/v1/sessions/{sessionId}/materials` | ✅ | Upload session material |
| `DELETE` | `/api/v1/sessions/{sessionId}/materials/{materialId}` | ✅ | Delete session material |
| `PATCH` | `/api/v1/sessions/{sessionId}/materials/reorder` | ✅ | Reorder materials |

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

---

## 🔄 MediatR Pipeline

Every request flows through a carefully ordered chain of cross-cutting behaviors:

```
Request
  │
  ├─ 1. LoggingBehavior         → Logs start/end of every request
  ├─ 2. PerformanceBehavior     → Alerts on slow requests
  ├─ 3. AuthorizationBehavior   → Checks user permissions
  ├─ 4. ValidationBehavior      → Runs FluentValidation rules
  ├─ 5. CachingBehavior         → Returns cached response (Queries)
  ├─ 6. CacheInvalidationBehavior → Clears cache (Commands)
  └─ 7. TransactionBehavior     → Wraps handler in DB transaction
        │
        └─ Handler → Executes business logic
```

---

## 🧪 Testing

```bash
dotnet test tests/Skill-Loop.UnitTests
```

---

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## 📝 License

This project is licensed under the **MIT License** — see the [LICENSE](LICENSE) file for details.

---

<p align="center">
  Built with ❤️ as a Graduation Project
</p>
