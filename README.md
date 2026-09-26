<p align="center">
  <h1 align="center">🔄 Skill Loop</h1>
  <p align="center">
    <strong>Teach. Earn. Learn. Repeat.</strong><br/>
    A peer-to-peer skill-sharing platform where users teach what they know, earn credits,<br/>
    and spend them on the skills they want to master.
  </p>
  <p align="center">
    <img src="https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 10" />
    <img src="https://img.shields.io/badge/C%23-13-239120?style=for-the-badge&logo=csharp&logoColor=white" alt="C#" />
    <img src="https://img.shields.io/badge/SQL%20Server-2022-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white" alt="SQL Server" />
    <img src="https://img.shields.io/badge/Architecture-Clean-blueviolet?style=for-the-badge" alt="Clean Architecture" />
    <img src="https://img.shields.io/badge/License-MIT-yellow?style=for-the-badge" alt="License" />
  </p>
</p>

---

## 📖 Overview

**Skill Loop** is a graduation-project backend API that powers a **peer-to-peer skill-sharing mobile application**. Users can register, browse sessions created by other users, book sessions using an internal **credit system**, chat with instructors, and teach their own skills to earn credits.

The backend is built with **Clean Architecture** on **.NET 10**, supporting:
- **Two authentication flows**: Mobile users (register + email OTP verification) and Staff/Admin (invitation-based)
- **Credit-based economy**: Users earn credits by teaching → spend credits to learn
- **Real-time chat** via SignalR
- **Session booking** with date/time selection, wallet integration, and status management
- **Google OAuth** integration for both user types

> **Note on OTP**: OTP verification is done via **Email** (not SMS) to avoid phone verification costs. The service is designed as a separate, swappable module — it can be migrated to SMS-based OTP in the future without changing the rest of the codebase.

---

## ✨ Key Features

| Category | Features |
|---|---|
| 🔐 **User Authentication** | Register (Email/Password) · Email OTP Verification · Resend OTP · Login · Google OAuth · JWT Access & Refresh Tokens |
| 🔐 **Staff Authentication** | Staff Login (Email/Password) · Google OAuth · Invitation-based Onboarding |
| 👤 **Account Management** | Profile CRUD · Avatar Upload · Password Change · Forgot/Reset Password (via OTP) · Activate/Deactivate Users |
| 🎤 **Instructor Profiles** | Create/Update Profile · Approval Workflow · Reviews & Ratings · Instructor Stats (Sessions Completed, Credits Earned) |
| 💰 **Wallets & Credits** | View Balance · Transaction History (Paginated) · Auto-Credit on Session Completion · Wallet Deduction on Booking · Optimistic Concurrency |
| 📚 **Courses & Enrollments** | Course CRUD · Sections & Lessons · Course Reviews · Bookmarks · Enrollment · Lesson Progress Tracking |
| 🎓 **Sessions** | Session CRUD · Status Management (Draft/Published) · Session Types (Online/Offline) · Session Materials (Google Drive) |
| 📅 **Bookings** | Create Booking (date + time) · Complete Booking · Cancel Booking · Wallet Integration · Self-booking Prevention |
| 💬 **Real-time Chat** | 1-on-1 Conversations · SignalR WebSocket · Read Receipts · Message Notifications |
| 📩 **Staff Invitations** | Admin sends invite via email → Staff accepts with password or Google account |
| 🔑 **Permission System** | Granular module-based permissions · Role–Permission assignment · Dynamic RBAC |
| 📱 **OTP Verification** | HMAC-hashed codes · Email delivery · Configurable expiry & cooldown · Max attempts lockout · Purposes: EmailVerification, PasswordReset |
| 📊 **Observability** | Structured logging (Serilog) · Correlation IDs · Performance tracking |
| ⚙️ **Background Jobs** | Outbox pattern with Hangfire for reliable domain event processing |

---

## 🏗️ Architecture

The project follows **Clean Architecture** (aka Onion Architecture) with strict dependency rules:

```
┌─────────────────────────────────────────────────┐
│                  Skill-Loop.Api                  │  ← Presentation (Controllers, Middlewares, SignalR)
├─────────────────────────────────────────────────┤
│             Skill-Loop.Application               │  ← Use Cases (CQRS via MediatR)
├─────────────────────────────────────────────────┤
│               Skill-Loop.Domain                  │  ← Core Entities, Enums, Domain Events
├─────────────────────────────────────────────────┤
│           Skill-Loop.Infrastructure              │  ← EF Core, Identity, Email, Jobs, Cache
└─────────────────────────────────────────────────┘
```

### Layer Responsibilities

- **Domain** — Pure business entities (`Course`, `Session`, `Booking`, `InstructorProfile`, `UserWallet`, `OtpVerification`), base entity types (`AuditableEntity`, `SoftDeleteEntity`), domain events, enums, and the `Result<T>` pattern for error handling.
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
- **OTP with HMAC Hashing** (Email-based verification)

### Infrastructure
- **Hangfire** — Background job processing & outbox consumer
- **MailKit** — SMTP email delivery (OTP codes, invitations, notifications)
- **Serilog** — Structured logging (Console + File sinks + enrichers)
- **UAParser** — User-agent detection
- **Google Drive API** — Session material storage

### API Documentation
- **Scalar** (OpenAPI/Swagger alternative)

### Real-time
- **SignalR** — WebSocket-based chat

---

## 📂 Project Structure

```
Skill-Loop/
├── Skill-Loop.slnx                    # Solution file
├── README.md
│
├── src/
│   ├── Skill-Loop.Api/                # 🌐 Presentation Layer
│   │   ├── Controllers/
│   │   │   ├── AuthController          #     All auth (Staff + User + OTP + Password)
│   │   │   ├── ProfileController       #     Current user profile
│   │   │   ├── UsersController         #     Admin user management
│   │   │   ├── StaffInvitationsController  # Invitation flow
│   │   │   ├── PermissionManagementController  # RBAC management
│   │   │   ├── CategoriesController    #     Categories CRUD
│   │   │   ├── CoursesController       #     Courses CRUD
│   │   │   ├── EnrollmentsController   #     Course enrollment & progress
│   │   │   ├── SessionsController      #     Sessions CRUD
│   │   │   ├── SessionMaterialsController  # Session file management
│   │   │   ├── BookingsController      #     Session booking & management
│   │   │   ├── InstructorProfilesController  # Instructor profiles & reviews
│   │   │   ├── WalletsController       #     Wallet balance & transactions
│   │   │   ├── ChatController          #     Real-time chat
│   │   │   └── SiteSettingsController  #     Site settings
│   │   ├── Contracts/                  #   Request/Response DTOs
│   │   ├── Middlewares/                #   CorrelationId, GlobalExceptionHandler
│   │   ├── Extensions/                #   Pipeline & DI extensions
│   │   └── appsettings.json           #   Configuration
│   │
│   ├── Skill-Loop.Application/        # 📋 Application Layer
│   │   ├── Features/
│   │   │   ├── Accounts/              #   Auth, Account Mgmt, Permissions, Invitations
│   │   │   │   ├── UserAuth/          #   RegisterUser, UserLogin, UserGoogleLogin, VerifyEmailOtp, ResendEmailOtp
│   │   │   │   ├── StaffAuth/         #   StaffLogin, StaffGoogleLogin
│   │   │   │   ├── Authentication/    #   RefreshToken, Logout
│   │   │   │   ├── AccountManagement/ #   ChangePassword, ForgotPassword, ResetPassword, Profile, Users
│   │   │   │   ├── StaffInvitations/  #   Send, Accept, AcceptWithGoogle, Validate
│   │   │   │   └── PermissionManagement/  # Roles & Permissions CRUD
│   │   │   ├── Bookings/             #   CreateBooking, CompleteBooking, CancelBooking
│   │   │   ├── Categories/            #   Categories Commands & Queries
│   │   │   ├── Courses/               #   Course Commands, Queries & Events
│   │   │   ├── Enrollments/           #   Enrollment & Lesson Progress
│   │   │   ├── Instructors/           #   Instructor Profile CRUD, Reviews, EventHandlers
│   │   │   ├── Wallets/               #   Wallet Queries, DTOs & EventHandlers
│   │   │   ├── Sessions/              #   Sessions & Materials Commands & Queries
│   │   │   ├── Chat/                  #   Chat Commands & Queries
│   │   │   └── SiteSettings/         #   Site settings Commands & Queries
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
│   │   │   ├── Booking/               #   Booking entity (with Factory Method + validation)
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
│   │   ├── Enums/                     #   OtpPurpose, SessionStatus, SessionType, BookingStatus, etc.
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
├── tests/
│   └── Skill-Loop.UnitTests/          # 🧪 Unit Tests
│
└── docs/
    └── ARCHITECTURE_GUIDE.md          # 📖 Detailed architecture & developer guide
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

  // Email (SMTP) — used for OTP delivery, invitations, notifications
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
| `POST` | `/api/v1/auth/user/register` | ❌ | Register a new user (email + password) |
| `POST` | `/api/v1/auth/user/verify-email` | ❌ | Verify email with OTP code |
| `POST` | `/api/v1/auth/user/resend-verification-code` | ❌ | Resend email OTP code |
| `POST` | `/api/v1/auth/user/login` | ❌ | User login (email + password) |
| `POST` | `/api/v1/auth/user/login/google` | ❌ | User login via Google OAuth |
| `POST` | `/api/v1/auth/staff/login` | ❌ | Staff login (email + password) |
| `POST` | `/api/v1/auth/staff/login/google` | ❌ | Staff login via Google OAuth |
| `POST` | `/api/v1/auth/refresh-token` | ❌ | Refresh access token |
| `POST` | `/api/v1/auth/logout` | ✅ | Revoke refresh token |
| `POST` | `/api/v1/auth/password/forgot` | ❌ | Request password reset OTP via email |
| `POST` | `/api/v1/auth/password/reset` | ❌ | Reset password with OTP code |

### 👤 Account Management

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/profile/me` | ✅ | Get current user profile |
| `PUT` | `/api/profile/me` | ✅ | Update profile info |
| `PUT` | `/api/profile/me/picture` | ✅ | Update avatar |
| `POST` | `/api/profile/me/change-password` | ✅ | Change current password |
| `GET` | `/api/users` | 🔒 Admin | List all users (paginated) |
| `GET` | `/api/users/{userId}` | 🔒 Admin | Get user by ID |
| `PUT` | `/api/users/{userId}/activate` | 🔒 Admin | Activate a user |
| `PUT` | `/api/users/{userId}/deactivate` | 🔒 Admin | Deactivate a user |
| `POST` | `/api/users/{userId}/roles` | 🔒 Admin | Assign role to user |
| `DELETE` | `/api/users/{userId}/roles/{roleName}` | 🔒 Admin | Remove role from user |

### 📩 Staff Invitations (`/api/staff-invitations`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/staff-invitations/send` | 🔒 Admin | Send invitation email |
| `GET` | `/api/staff-invitations/validate/{token}` | ❌ | Validate invitation token |
| `POST` | `/api/staff-invitations/accept` | ❌ | Accept with password |
| `POST` | `/api/staff-invitations/accept-google` | ❌ | Accept with Google account |

### 🔑 Permission Management (`/api/permission-management`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/permission-management/permissions` | ✅ | Get all permissions |
| `GET` | `/api/permission-management/roles` | ✅ | Get all roles with permissions |
| `GET` | `/api/permission-management/roles/{roleId}` | ✅ | Get permissions for a role |
| `POST` | `/api/permission-management/roles/{roleId}/permissions/{permissionId}/assign` | ✅ | Assign permission to role |
| `POST` | `/api/permission-management/roles/{roleId}/permissions/{permissionId}/remove` | ✅ | Remove permission from role |
| `POST` | `/api/permission-management/roles/{roleId}/permissions/update` | ✅ | Batch update role permissions |

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

### 🎓 Sessions (`/api/sessions`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/sessions` | ✅ | Create a new session |
| `GET` | `/api/sessions` | ❌ | List sessions (paginated) |
| `GET` | `/api/sessions/{id}` | ❌ | Get session by ID |
| `PUT` | `/api/sessions/{id}` | ✅ | Update a session |
| `DELETE` | `/api/sessions/{id}` | ✅ | Delete a session |
| `PATCH` | `/api/sessions/{id}/status` | ✅ | Change session status |

### 📅 Bookings (`/api/v1/bookings`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/v1/bookings` | ✅ | Create a booking (select date + time) |
| `PATCH` | `/api/v1/bookings/{id}/complete` | ✅ | Mark booking as completed |
| `PATCH` | `/api/v1/bookings/{id}/cancel` | ✅ | Cancel a booking |

### 📚 Courses & Enrollments

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/courses` | ✅ | Create course |
| `GET` | `/api/courses` | ❌ | List courses (paginated) |
| `GET` | `/api/courses/{id}` | ❌ | Get course details |
| `POST` | `/api/enrollments` | ✅ | Enroll in a course |
| `GET` | `/api/enrollments/me` | ✅ | Get enrolled courses |

### 💰 Wallets (`/api/wallets`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/wallets/me` | ✅ | Get current user's wallet balance |
| `GET` | `/api/wallets/me/transactions` | ✅ | Get transaction history (paginated) |

### 💬 Chat (`/api/chat`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/chat/conversations` | ✅ | Start a new conversation |
| `GET` | `/api/chat/conversations` | ✅ | Get my conversations |
| `GET` | `/api/chat/conversations/{id}/messages` | ✅ | Get conversation messages |
| `POST` | `/api/chat/conversations/{id}/messages` | ✅ | Send a message |
| `PATCH` | `/api/chat/conversations/{id}/read` | ✅ | Mark conversation as read |

> **SignalR Hub**: `/hubs/chat` — Real-time message notifications & read receipts

> **Legend:** ❌ Public · ✅ Authenticated · 🔒 Admin Only

---

## 🔄 MediatR Pipeline

Every request flows through a carefully ordered chain of cross-cutting behaviors:

```
Request
  │
  ├─ 1. LoggingBehavior         → Logs start/end of every request
  ├─ 2. PerformanceBehavior     → Alerts on slow requests (>800ms)
  ├─ 3. AuthorizationBehavior   → Checks user permissions
  ├─ 4. ValidationBehavior      → Runs FluentValidation rules
  ├─ 5. CachingBehavior         → Returns cached response (Queries)
  ├─ 6. CacheInvalidationBehavior → Clears cache (Commands)
  └─ 7. TransactionBehavior     → Wraps handler in DB transaction
        │
        └─ Handler → Executes business logic
```

---

## 📱 Mobile App Screen Coverage

The backend supports all screens in the SkillLoop mobile application:

| Screen | Backend Support | Details |
|--------|----------------|---------|
| Splash / Onboarding | N/A | Client-side only |
| **Login** | ✅ | `POST /api/v1/auth/user/login` + Google OAuth |
| **Sign Up** | ✅ | `POST /api/v1/auth/user/register` → Email OTP verification |
| **OTP Verification** | ✅ | `POST /api/v1/auth/user/verify-email` (via Email, not phone) |
| **Forgot Password** | ✅ | `POST /api/v1/auth/password/forgot` → OTP via email |
| **Reset Password** | ✅ | `POST /api/v1/auth/password/reset` (with OTP code) |
| **Home** | ✅ | Sessions list + Wallet balance + Categories |
| **Explore Skills** | ✅ | `GET /api/sessions` (paginated + filtered by category) |
| **Session Details** | ✅ | `GET /api/sessions/{id}` + Instructor profile |
| **Book a Session** | ✅ | `POST /api/v1/bookings` (date + time + wallet deduction) |
| **Payment / Credits** | ✅ | Wallet-based credit system (auto-deduction on booking) |
| **Booking Confirmed** | ✅ | Booking status flow (Confirmed → Completed/Cancelled) |
| **My Profile** | ✅ | `GET /api/profile/me` + Wallet + Instructor stats |
| **Messages** | ✅ | `GET /api/chat/conversations` + SignalR real-time |
| **Chat** | ✅ | `POST /api/chat/conversations/{id}/messages` + read receipts |
| **Wallet** | ✅ | `GET /api/wallets/me` + transaction history |
| **Teach** | ✅ | Session CRUD + Instructor profile + Booking management |

> **⚠️ OTP Note**: The UI shows "Check your phone" for OTP, but the backend sends OTP codes via **Email**. This is by design — phone SMS verification costs money, so email is used as a free alternative. The OTP service is decoupled and can be swapped to SMS in the future.

---

## 🧪 Testing

```bash
dotnet test tests/Skill-Loop.UnitTests
```

### Test Coverage

| Module | Tests |
|--------|-------|
| Auth (Staff Login, Refresh, Logout) | ✅ |
| Account Management (Profile, Password, Users) | ✅ |
| Permissions | ✅ |
| Staff Invitations | ✅ |
| Courses (Aggregate) | ✅ |
| Enrollments & Wallets | ✅ |
| Sessions (CRUD + Validators) | ✅ |
| Session Materials | ✅ |
| Cache Service | ✅ |
| Notifications | ✅ |

---

## 📋 Feature Completeness

### ✅ Fully Implemented

- Authentication (User + Staff + Google OAuth)
- Email OTP Verification (Register + Password Reset)
- Account & Profile Management
- Staff Invitations (RBAC)
- Permission Management
- Categories
- Courses & Enrollments
- Sessions & Materials (with Google Drive)
- Bookings (Create, Complete, Cancel)
- Wallet & Credit System
- Real-time Chat (SignalR)
- Instructor Profiles & Reviews
- Site Settings
- Background Jobs (Outbox Pattern)
- Structured Logging & Observability

### 🔜 Planned / Future Enhancements

| Feature | Priority | Notes |
|---------|----------|-------|
| Push Notifications (Firebase FCM) | 🟡 Medium | SignalR + Email exist, push not yet |
| Payment Gateway Integration | 🟡 Medium | For purchasing credits with real money |
| SMS-based OTP | 🟢 Low | Currently email-based; service is swappable |
| Promo Codes | 🟢 Low | Permission exists (`PromoCodes.Manage`) but feature not built |
| Credit Packages | 🟢 Low | Permission exists (`Packages.Manage`) but feature not built |

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
  Built with ❤️ as a Graduation Project<br/>
  <strong>Teach. Earn. Learn. Repeat.</strong>
</p>
