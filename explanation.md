# Skill Loop — Business & Technical Explanation

## 📌 Business Overview

**Skill Loop** is a peer-to-peer skill-sharing platform where users can both **learn** (enroll in courses, book live sessions) and **teach** (create courses, host live sessions). The platform uses a **virtual credit economy** — users earn credits by teaching and spend credits by learning.

### Core Value Proposition
| Actor | Actions |
|-------|---------|
| **Learner** | Browse courses, enroll (pay credits), track lesson progress, book live 1-on-1 sessions, chat with instructors, leave reviews |
| **Instructor** | Create courses with sections/lessons, host live sessions (online/offline), set prices, earn credits, build reputation via ratings |
| **Admin/Staff** | Manage users, roles, permissions, categories, site settings, approve instructors, send staff invitations |

### Key Business Flows
1. **Discover & Learn** → Search courses → View details → Enroll (atomic credit deduction) → Complete lessons → Earn progress %
2. **Book Live Session** → Browse instructor sessions → Select slot → Pay credits → Attend → Instructor gets paid
3. **Teach & Earn** → Create instructor profile → Create courses/sessions → Get bookings/enrollments → Earn credits → Withdraw (future)
4. **Wallet Economy** → Credits flow: Learner pays → Instructor earns → Platform takes no cut (currently)

---

## 🏗️ Technical Architecture

### Clean Architecture (4 Layers)
```
┌─────────────────────────────────────────────────┐
│              Skill-Loop.Api                      │  ← Presentation (Controllers, SignalR, Middleware)
├─────────────────────────────────────────────────┤
│            Skill-Loop.Application                │  ← Use Cases (CQRS via MediatR)
├─────────────────────────────────────────────────┤
│              Skill-Loop.Domain                   │  ← Core Entities, Value Objects, Domain Events
├─────────────────────────────────────────────────┤
│           Skill-Loop.Infrastructure              │  ← EF Core, Identity, Email, Jobs, Cache, Storage
└─────────────────────────────────────────────────┘
```

**Dependency Rule**: `Api → Application → Domain ← Infrastructure`

---

## 📂 Layer-by-Layer File Breakdown

### 1. Skill-Loop.Domain (Core Business Logic)

#### Entities (`Domain/Entities/`)
| File | Purpose |
|------|---------|
| `Course.cs` | Aggregate root: title, description, price (credits), level, status, instructor, category, sections/lessons, reviews, bookmarks |
| `Section.cs` | Logical grouping of lessons within a course |
| `Lesson.cs` | Individual lesson with video/PDF resources, duration, order |
| `Category.cs` | Course categorization (e.g., Programming, Design) |
| `CourseReview.cs` | Student rating (1-5) + comment on a course |
| `CourseBookmark.cs` | User's saved courses |
| `Enrollment.cs` | User's enrollment in a course: credits paid, status, progress %, last watched lesson |
| `LessonProgress.cs` | Tracks completion of individual lessons |
| `InstructorProfile.cs` | Instructor's public profile: headline, bio, approval status, rating, stats (sessions completed, credits earned) |
| `InstructorReview.cs` | Student rating + comment on an instructor |
| `Session.cs` | Live teaching slot: schedule, duration, price, location (online/offline), capacity, bookings |
| `SessionMaterial.cs` | Files attached to a session (PDF, video, etc.) — stored on Google Drive |
| `Booking.cs` | Learner's reservation of a session: status lifecycle, price snapshot, refund logic |
| `UserWallet.cs` | User's credit balance with optimistic concurrency (RowVersion) |
| `WalletTransaction.cs` | Ledger entry: credit deduction, refund, reward |
| `Conversation.cs` | 1-on-1 chat between two users |
| `ChatMessage.cs` | Individual message in a conversation |
| `StaffInvitation.cs` | Admin invitation for staff onboarding |
| `OtpVerification.cs` | OTP codes for email verification/password reset |
| `SiteSettings.cs` | Global platform configuration |
| `ApplicationUser/ApplicationRole/RefreshToken` | Identity extensions |

#### Value Objects (`Domain/Entities/Courses/ValueObjects/`)
| File | Purpose |
|------|---------|
| `CoursePrice.cs` | Encapsulates price in credits; `IsFree` when 0 |
| `CourseRating.cs` | Average rating + review count; auto-recalculates |
| `VideoResource.cs` | Video URL, duration, resolution, provider ID |
| `PdfAttachment.cs` | PDF file name, storage URL, size |

#### Enums (`Domain/Enums/`)
`CourseLevel`, `CourseStatus`, `EnrollmentStatus`, `TransactionType`, `SessionStatus`, `BookingStatus`, `MaterialType`, `OtpPurpose`, `SessionLocationType`

#### Domain Events (`Domain/Entities/*/Events/`)
Events raised by entities and processed asynchronously via Outbox pattern:
- `CourseCreated/Updated/PublishedDomainEvent`
- `CourseEnrolled/LessonCompleted/CourseCompletedDomainEvent`
- `SessionMaterialUploadedEvent`, `SessionCompletedDomainEvent`
- `BookingCreatedDomainEvent`, `BookingCancelledDomainEvent`
- `WalletBalanceDeductedDomainEvent`
- `StaffInvitationCreatedEvent`

#### Base Classes (`Domain/Common/Entities/`)
`Entity` → `BaseEntity` (Guid V7) → `AuditableEntity` (CreatedAt/By, UpdatedAt/By) → `SoftDeleteEntity` (IsDeleted, DeletedAt/By)

#### Result Pattern (`Domain/Common/Results/`)
`Result<T>` / `Result` with `Error(Code, Description, ErrorType)` — maps to HTTP status codes

---

### 2. Skill-Loop.Application (Use Cases / CQRS)

#### Structure: `Features/{Feature}/{Commands|Queries|EventHandlers|DTOs|Shared}/`

#### Key Features & Their Files

| Feature | Commands | Queries | Event Handlers |
|---------|----------|---------|----------------|
| **Accounts/Auth** | StaffLogin, StaffGoogleLogin, RegisterUser, VerifyEmailOtp, RefreshToken, Logout, ForgotPassword, ResetPassword, ChangePassword, SendInvitation, AcceptInvitation, ActivateUser, DeactivateUser, AssignRole, RemoveRole, UpdateMyProfile, UpdateMyProfilePicture | GetMyProfile, GetAllUsers, GetAllPermissions, GetRolePermissions, GetAllRolesWithPermissions, ValidateInvitation | StaffInvitationCreated |
| **Categories** | CreateCategory, UpdateCategory, DeleteCategory | GetCategories | — |
| **Courses** | CreateCourse, AddLesson, PublishCourse, AddCourseReview, ToggleCourseBookmark | GetCourseById, GetCoursesPaged (search/filter/sort/cache) | CourseInvalidationHandler (cache clear) |
| **Enrollments** | EnrollInCourse (atomic wallet deduction), UpdateLessonProgress | GetUserEnrolledCourses | — |
| **Instructors** | CreateMyInstructorProfile, UpdateMyInstructorProfile, ChangeInstructorApprovalStatus, AddInstructorReview, UpdateInstructorReview, RemoveInstructorReview | GetInstructorsPaged, GetInstructorProfileByUserId, GetInstructorFullProfileByUserId | CourseEnrolled (credits earned), SessionCompleted (sessions++) |
| **Wallets** | — | GetMyWallet (lazy creation), GetMyWalletTransactionsPaged | CreditInstructorWallet (course sales), CreditInstructorWalletOnSessionCompleted (live sessions) |
| **Sessions** | CreateSession, UpdateSession, DeleteSession, ChangeSessionStatus | GetSessionById, GetSessionsPaged (filter by instructor/status/date/bookable), GetMySessionsPaged (instructor dashboard) | — |
| **Session Materials** | UploadSessionMaterial, DeleteSessionMaterial, ReorderSessionMaterials | GetSessionMaterials, GetSessionMaterialDownloadInfo | SessionMaterialUploaded |
| **Bookings** | CreateBooking (atomic payment + capacity check), CancelBooking (refund), ChangeBookingStatus (instructor lifecycle) | GetBookingById, GetMyBookings, GetSessionBookings | — |
| **Chat** | StartConversation, SendMessage, MarkConversationRead | GetMyConversations, GetConversationMessages | — |
| **SiteSettings** | UpdateSiteSettings | GetSiteSettings | — |

#### Pipeline Behaviors (MediatR) — Order Matters
1. `LoggingBehavior` — Logs request start/end with CorrelationId
2. `PerformanceBehavior` — Warns if > 800ms
3. `AuthorizationBehavior` — Checks `[Permission]` attribute via `ICurrentUser`
4. `ValidationBehavior` — Runs FluentValidation rules
5. `CachingBehavior` — Cache-aside for `ICacheableQuery`
6. `CacheInvalidationBehavior` — Clears cache prefixes for `ICacheInvalidatorCommand`
7. `TransactionBehavior` — Wraps commands in DB transaction

#### Cross-Cutting Abstractions (`Common/Abstractions/`)
- `ICacheService`, `ICacheableQuery`, `ICacheInvalidatorCommand`
- `IEmailSender`, `IFileStorage`, `ICourseContentStorage` (Google Drive)
- `IJobScheduler` (Hangfire), `IChatNotifier` (SignalR)
- `IApplicationUrlService`, `IClientContext`

---

### 3. Skill-Loop.Infrastructure (External Implementations)

#### Persistence (EF Core + SQL Server)
| File | Purpose |
|------|---------|
| `AppDbContext.cs` | DbContext with all DbSets; applies configurations + soft-delete global filter |
| `Configurations/*.cs` | EF Core fluent API for each entity (31 configuration files) |
| `Interceptors/` | `AuditableEntityInterceptor`, `SoftDeleteInterceptor`, `InsertOutboxMessagesInterceptor` |
| `Outbox/OutboxMessage.cs` | Reliable event publishing table |
| `Seed/ContextSeed.cs` | Creates roles, permissions, role-permission mappings on startup |
| `Transaction/EfTransactionManager.cs` | Unit of Work wrapper |

#### Identity & Security
| File | Purpose |
|------|---------|
| `Authentication/` | StaffAuthService, UserAuthService, GoogleAuthProvider |
| `Authorization/` | PermissionService (checks user permissions) |
| `CurrentUser/` | `ICurrentUser` implementation (claims → user context) |
| `Invitations/` | Invitation token generation/validation |
| `Security/` | Password hashing, HMAC for OTP |
| `Tokens/` | `JwtTokenGenerator` (access + refresh), `RefreshTokenService` |
| `UserManagement/` | User CRUD, avatar, roles, activation |

#### External Services
| File | Purpose |
|------|---------|
| `Email/SmtpEmailSender.cs` | MailKit SMTP with HTML templates |
| `Email/EmailTemplateEngine.cs` | Razor-like template rendering |
| `FileStorage/LocalFileStorage.cs` | Local disk file storage |
| `Storage/GoogleDriveContentStorage.cs` | Google Drive API for course/session materials |
| `Cache/RedisCacheService.cs` | StackExchange.Redis implementation |
| `Cache/MemoryCacheService.cs` | Fallback in-memory cache |
| `Client/` | HttpClientContext, GeoLocationService, UserAgentParser |
| `Jobs/HangfireJobScheduler.cs` | `IJobScheduler` implementation |

#### Background Jobs
| File | Purpose |
|------|---------|
| `ProcessOutboxMessagesJob.cs` | Runs every 5s: reads Outbox → publishes via MediatR |
| `RefreshDriveQuotaJob.cs` | Monitors Google Drive storage quota |

#### Dependency Injection (`DependencyInjection/`)
Modular registration: `AddPersistence`, `AddIdentityServices`, `AddJwtAuthentication`, `AddExternalAuth`, `AddMail`, `AddFileStorage`, `AddGoogleDriveStorage`, `AddCaching`, `AddHangfireJobs`, `AddOtpService`, `AddBaseUrl`, `AddDatabaseSeeder`

---

### 4. Skill-Loop.Api (Presentation Layer)

#### Controllers (`Controllers/`)
| Controller | Route | Responsibility |
|------------|-------|----------------|
| `AuthController` | `/api/auth` | Login, Google login, register, OTP, refresh, logout, password reset |
| `ProfileController` | `/api/profile` | Current user profile CRUD, avatar, password change |
| `UsersController` | `/api/users` | Admin: list users, activate/deactivate, assign roles |
| `PermissionManagementController` | `/api/permission-management` | Role-permission matrix management |
| `StaffInvitationsController` | `/api/staff-invitations` | Send/validate/accept invitations |
| `CategoriesController` | `/api/categories` | Category CRUD |
| `CoursesController` | `/api/courses` | Course CRUD, publish, lessons, reviews, bookmarks |
| `EnrollmentsController` | `/api/enrollments` | Enroll, my courses, lesson progress |
| `InstructorProfilesController` | `/api/instructor-profiles` | Instructor CRUD, approval, reviews |
| `WalletsController` | `/api/wallets` | Balance, transaction history |
| `SessionsController` | `/api/sessions` | Session CRUD, scheduling, instructor dashboard |
| `SessionMaterialsController` | `/api/sessions/{id}/materials` | Upload/download/reorder materials |
| `BookingsController` | `/api/bookings` | Create/cancel/change status, my bookings, session bookings |
| `ChatController` | `/api/chat` | Conversations, messages, mark read |
| `SiteSettingsController` | `/api/site-settings` | Get/update site settings |

#### SignalR (`Hubs/`)
- `ChatHub` at `/hubs/chat` — Real-time 1-on-1 messaging
- `SignalRChatNotifier` — Implements `IChatNotifier` for server→client push
- `ChatUserIdProvider` — Maps JWT claims to SignalR connection

#### Middleware & Extensions
- `CorrelationIdMiddleware` — Adds `X-Correlation-ID` to every request
- `GlobalExceptionHandler` — Maps exceptions to ProblemDetails
- `ResultExtensions` — Maps `ErrorType` → HTTP status (Validation→400, NotFound→404, etc.)
- `PipelineExtensions` — Configures Hangfire recurring jobs, SignalR, CORS, Scalar/OpenAPI
- `LoggingExtensions` — Serilog configuration (Console + File + Enrichers)

#### Contracts (`Contracts/`)
Request DTOs per feature (e.g., `CreateCourseRequest`, `EnrollInCourseRequest`, `CreateBookingRequest`) — sealed records, used only in controllers

---

## 🔄 Key Technical Flows

### 1. Atomic Enrollment (Course)
```
POST /api/enrollments/enroll
  → EnrollInCourseCommand
  → Handler: Check wallet balance → UserWallet.DeductCredits() → Enrollment.Create()
  → Raises CourseEnrolledDomainEvent
  → Outbox → CreditInstructorWalletEventHandler → Instructor wallet + stats
```

### 2. Live Session Booking
```
GET /api/sessions?bookableOnly=true → Lists sessions with AvailableSlots
POST /api/bookings
  → CreateBookingCommand
  → Handler: Validate session (published, future, capacity, not self, no duplicate)
  → UserWallet.DeductCredits() → Booking.Create() → BookingCreatedDomainEvent
  → Outbox → (future: confirmation email)
PATCH /api/bookings/{id}/status?status=Completed
  → ChangeBookingStatusCommand (instructor only)
  → Booking.Complete() → SessionCompletedDomainEvent
  → Outbox → CreditInstructorWalletOnSessionCompletedEventHandler → Instructor wallet + stats
```

### 3. Domain Event → Outbox → Handler
```
Entity.AddDomainEvent(event)
  → SaveChanges()
  → InsertOutboxMessagesInterceptor: Serialize event → Insert OutboxMessage
  → ProcessOutboxMessagesJob (every 5s, Hangfire)
  → Deserialize → DomainEventNotification<T> → MediatR.Publish()
  → EventHandler executes (e.g., credit instructor wallet, invalidate cache)
```

### 4. Caching & Invalidation
- Queries implement `ICacheableQuery` with `CacheKey`, sliding/absolute expiration
- Commands implement `ICacheInvalidatorCommand` with `CacheKeys` to clear
- `CourseInvalidationHandler` clears `courses:paged:*` and `categories:all` on publish

---

## 🗄️ Database Schema (Key Tables)

| Table | Key Columns |
|-------|-------------|
| `Courses` | Id, Title, Description, CreditsPrice, Level, Status, InstructorId, CategoryId, AverageRating, TotalReviews, IsDeleted |
| `Sections` | Id, CourseId, Title, Order |
| `Lessons` | Id, SectionId, Title, VideoUrl, Duration, Order |
| `Enrollments` | Id, UserId, CourseId, CreditsPaid, Status, ProgressPercentage, LastWatchedLessonId |
| `UserWallets` | Id, UserId (UK), Balance, RowVersion (concurrency) |
| `WalletTransactions` | Id, UserWalletId, Amount, Type (CreditDeduction/Refund/Reward), Description |
| `Sessions` | Id, InstructorId, Title, Description, ScheduledAtUtc, DurationMinutes, CreditsPrice, LocationType, LocationDetails, MaxParticipants, Status |
| `Bookings` | Id, SessionId, LearnerUserId, Status, PriceInCredits, ScheduledAtUtc, BookedAtUtc, StartedAtUtc, CompletedAtUtc, CancelledAtUtc, CancellationReason |
| `SessionMaterials` | Id, SessionId, FileName, StorageUrl, FileSize, MaterialType, Order, GoogleDriveFileId |
| `Conversations` | Id, ParticipantOneId, ParticipantTwoId, LastMessageAt, LastMessagePreview |
| `ChatMessages` | Id, ConversationId, SenderId, Content, SentAt, ReadAt |
| `InstructorProfiles` | Id, UserId (1-1), Headline, Bio, IsApproved, Rating, SessionsCompleted, CreditsEarned |
| `InstructorReviews` | Id, InstructorProfileId, LearnerUserId, Rating, Comment, IsDeleted |
| `TbPermission` | Id, Name (e.g., "Categories.Manage") |
| `TbRolePermission` | RoleId, PermissionId |
| `OutboxMessages` | Id, Type, Content (JSON), OccurredOnUtc, ProcessedOnUtc, RetryCount |

---

## 🧪 Testing

- **Unit Tests**: `tests/Skill-Loop.UnitTests/`
  - `InMemoryDbContextHelper` / `InMemoryAppDbContext` for EF Core InMemory testing
  - Tests for: Booking lifecycle, Session scheduling/bookability, Wallet transactions, Domain entities, Handlers, Validators
  - `RefreshDriveQuotaJobTests`, `NotificationServiceTests`, `CacheServiceTests`

---

## ⚙️ Tech Stack Summary

| Category | Technology |
|----------|------------|
| Runtime | .NET 10, C# 13 |
| Framework | ASP.NET Core 10 (Minimal Hosting) |
| Database | SQL Server, EF Core 10 (Code-First) |
| Auth | ASP.NET Core Identity, JWT Bearer, Google OAuth, OTP (HMAC) |
| Patterns | Clean Architecture, CQRS (MediatR), DDD, Outbox Pattern |
| Validation | FluentValidation 12 |
| Mapping | AutoMapper 16 |
| Caching | Redis (StackExchange.Redis) + MemoryCache fallback |
| Background Jobs | Hangfire (SQL Server storage) |
| Real-time | SignalR (WebSocket) |
| Email | MailKit (SMTP) |
| File Storage | Local + Google Drive API |
| Logging | Serilog (Console, File, Enrichers) |
| API Docs | Scalar (OpenAPI) |
| Testing | xUnit, EF Core InMemory, Moq |

---

## 📈 Current Implementation Status (per Gap Analysis)

| Module | Status |
|--------|--------|
| Authentication (Staff + User) | ✅ Implemented |
| User/Profile Management | ✅ Implemented |
| RBAC Permission System | ✅ Implemented |
| OTP Verification | ✅ Implemented |
| Staff Invitations | ✅ Implemented |
| Site Settings | ✅ Implemented |
| Categories | ✅ Implemented |
| Courses (CRUD, Search, Cache) | ✅ Implemented |
| Enrollments (Atomic + Progress) | ✅ Implemented |
| Instructor Profiles & Reviews | ✅ Implemented |
| Wallets (Balance, History, Earning, Refunds) | ✅ Partial — **No Buy Credits / Promo Codes** |
| Sessions (Scheduling, Bookability) | ✅ Implemented |
| Bookings (Full Lifecycle + Payment) | ✅ Implemented |
| Session Materials (Google Drive) | ✅ Implemented |
| Chat (REST + SignalR) | ✅ Implemented |
| Reviews (Course + Instructor) | ✅ Implemented — **No Session Reviews** |
| Background Jobs / Outbox | ✅ Implemented |
| Redis Caching | ✅ Implemented — **Startup bug: MemoryCache fallback commented out** |

---

## 🎯 Remaining Backlog (Prioritized)

### Priority 1 — Wallet Economy
- `BuyCreditsCommand` + `Payment` entity + payment gateway integration
- `ApplyPromoCodeCommand` + `PromoCode` entity
- WalletController endpoints for purchase/promo

### Priority 2 — Teaching Dashboard
- Aggregated earnings summary (per-period, per-course)
- Per-course revenue stats

### Priority 3 — Polish & Extensions
- Session reviews (`SessionReview` entity + commands/queries)
- Booking notifications (email on create, reminder job for upcoming)
- Session "requires instructor approval" flag → `BookingStatus.Pending`
- Unit test coverage for Wallet buy/promo, Chat
- Fix Redis fallback registration in `AddCaching.cs`
- Mobile auth: Apple, Facebook providers
- Push notifications (FCM) for offline chat/booking alerts

---

## 🚀 Getting Started

```bash
# 1. Configure appsettings.json (DB, JWT, SMTP, Google OAuth)
# 2. Apply migrations
cd src/Skill-Loop.Api
dotnet ef database update --project ../Skill-Loop.Infrastructure

# 3. Run
dotnet run --project src/Skill-Loop.Api

# API: https://localhost:7271
# Docs: https://localhost:7271/scalar/v1
# Hangfire: https://localhost:7271/hangfire
```

---

## 📁 Project Structure Recap

```
Skill-Loop/
├── src/
│   ├── Skill-Loop.Api/           # Controllers, Contracts, Middleware, SignalR, Program.cs
│   ├── Skill-Loop.Application/   # Features (CQRS), Behaviors, Abstractions, Errors, Pagination
│   ├── Skill-Loop.Domain/        # Entities, ValueObjects, Enums, Events, Base Classes, Results
│   └── Skill-Loop.Infrastructure/# Persistence, Identity, External Services, Jobs, DI
├── tests/
│   └── Skill-Loop.UnitTests/     # Unit tests with InMemory EF Core
├── docs/                         # Architecture, Subsystem docs, Gap Analysis
└── Skill-Loop.slnx               # Solution file
```

---

*Generated from codebase analysis on 2026-09-26*