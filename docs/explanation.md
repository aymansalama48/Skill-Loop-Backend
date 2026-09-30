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
| `InstructorAvailability.cs` | Recurring availability slot (day of week + time range) for an instructor |
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
| `SupportQuestion.cs` | FAQ entry / support request: question, answer, publication + email state |
| `Notification.cs` | In-app notification (currently created by chat events) |

> Identity models are **not** in the Domain layer: `ApplicationUser` / `ApplicationRole` / `RefreshToken` / `TbPermission` / `TbRolePermission` live in `Skill-Loop.Infrastructure/Persistence/IdentityModels/`.

#### Value Objects (`Domain/Entities/Courses/ValueObjects/`)
| File | Purpose |
|------|---------|
| `CoursePrice.cs` | Encapsulates price in credits; `IsFree` when 0 |
| `CourseRating.cs` | Average rating + review count; auto-recalculates |
| `VideoResource.cs` | Video URL, duration, resolution, provider ID |
| `PdfAttachment.cs` | PDF file name, storage URL, size |

#### Enums (`Domain/Enums/`)
`CourseLevel`, `CourseStatus`, `EnrollmentStatus`, `TransactionType`, `SessionStatus`, `SessionType`, `SessionLocationType`, `BookingStatus`, `MaterialType`, `OtpPurpose`

#### Domain Events (`Domain/Entities/*/Events/`)
Events raised by entities and processed asynchronously via Outbox pattern:
- `CourseCreated/Updated/PublishedDomainEvent`
- `CourseEnrolled/LessonCompleted/CourseCompletedDomainEvent`
- `SessionMaterialUploadedEvent`, `SessionCompletedDomainEvent`
- `BookingCreatedDomainEvent`, `BookingCancelledDomainEvent`
- `WalletBalanceDeductedDomainEvent`, `WalletBalanceRefundedDomainEvent`
- `StaffInvitationCreatedEvent`
- `ChatMessageSentEvent`

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
| **Accounts/Auth** | StaffLogin, StaffGoogleLogin, UserLogin, UserGoogleLogin, RegisterUser, VerifyEmailOtp, ResendEmailOtp, RefreshToken, Logout, ForgotPassword, ResetPassword, ChangePassword, SendInvitation, AcceptInvitation, AcceptInvitationWithGoogle, ActivateUser, DeactivateUser, AssignRoleToUser, RemoveRoleFromUser, AssignPermissionToRole, RemovePermissionFromRole, UpdateRolePermissions, UpdateMyProfile, UpdateMyProfilePicture | GetMyProfile, GetAllUsers, GetUserById, GetAllPermissions, GetRolePermissions, GetAllRolesWithPermissions, ValidateInvitation | StaffInvitationCreated |
| **Categories** | CreateCategory, UpdateCategory, DeleteCategory | GetCategories | — |
| **Courses** | CreateCourse, AddLesson, PublishCourse, AddCourseReview, ToggleCourseBookmark | GetCourseById, GetCoursesPaged (search/filter/sort/cache) | CourseInvalidationHandler (cache clear) |
| **Enrollments** | EnrollInCourse (atomic wallet deduction), UpdateLessonProgress | GetUserEnrolledCourses | — |
| **Instructors** | CreateMyInstructorProfile, UpdateMyInstructorProfile, ChangeInstructorApprovalStatus, AddInstructorAvailability, RemoveInstructorAvailability, AddInstructorReview, UpdateInstructorReview, RemoveInstructorReview | GetInstructorsPaged, GetInstructorProfileByUserId, GetInstructorFullProfileByUserId | CourseEnrolled (credits earned), SessionCompleted (sessions++) |
| **Wallets** | — | GetMyWallet (lazy creation), GetMyWalletTransactionsPaged | CreditInstructorWallet (course sales), CreditInstructorWalletOnSessionCompleted (live sessions) |
| **Sessions** | CreateSession, UpdateSession, DeleteSession, ChangeSessionStatus | GetSessionById, GetSessionsPaged (filter by instructor/status/date/bookable), GetMySessionsPaged (instructor dashboard) | — |
| **Session Materials** | UploadSessionMaterial, DeleteSessionMaterial, ReorderSessionMaterials | GetSessionMaterials, GetSessionMaterialDownloadInfo | SessionMaterialUploaded |
| **Bookings** | CreateBooking (atomic payment + capacity check), CancelBooking (refund), ChangeBookingStatus (instructor lifecycle), CompleteBooking (no controller endpoint yet) | GetBookingById, GetMyBookings, GetSessionBookings | — |
| **Chat** | StartConversation, SendMessage, MarkConversationRead | GetMyConversations, GetConversationMessages | ChatMessageSent (creates in-app notification) |
| **Notifications** | MarkNotificationRead, MarkAllNotificationsRead | GetMyNotifications, GetUnreadNotificationCount | — |
| **Support** | SubmitContactForm, SendFaqAnswerEmail, AnswerSupportQuestion, CreateSupportQuestion, UpdateSupportQuestion, SetSupportQuestionPublication, DeleteSupportQuestion | GetPublishedSupportQuestionsPaged, GetSupportQuestionById, GetSupportQuestionsPaged, GetSupportQuestionDetails, GetMySupportQuestions | — |
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
- `ISupportRequestNotifier`, `ISupportAnswerNotifier` (support emails, split per ISP)
- `ISiteSettingsService`, `IGeoLocationService`, `IUserAgentParser`
- `IApplicationUrlService`, `IClientContext`

---

### 3. Skill-Loop.Infrastructure (External Implementations)

#### Persistence (EF Core + SQL Server)
| File | Purpose |
|------|---------|
| `AppDbContext.cs` | DbContext with all DbSets; applies configurations + soft-delete global filter |
| `Configurations/*.cs` | EF Core fluent API for each entity (21 files, 29 `IEntityTypeConfiguration` classes) |
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
| `ProcessOutboxMessagesJob.cs` | Runs every 5s: reads Outbox → publishes via MediatR (the only `RecurringJob` actually scheduled) |
| `RefreshDriveQuotaJob.cs` | Intended Google Drive quota monitor — **registered in DI but never scheduled**, so it does not currently run |

#### Dependency Injection (`DependencyInjection/`)
Modular registration inside `AddInfrastructure()`: `AddPersistence`, `AddIdentityServices`, `AddJwtAuthentication`, `AddExternalAuth`, `AddMail`, `AddFileStorage`, `AddGoogleDriveStorage`, `AddCaching`, `AddHangfireJobs`, `AddOtpService`, `AddBaseUrl`.

`AddDatabaseSeeder` is **not** part of that chain — it's an `IApplicationBuilder` extension (`AddDatabaseSeeder.cs:17`) called directly as `app.SeedDatabaseAsync()` from `Program.cs:16`.

---

### 4. Skill-Loop.Api (Presentation Layer)

#### Controllers (`Controllers/`)
| Controller | Route | Responsibility |
|------------|-------|----------------|
| `AuthController` | `/api/v1/auth` | Login, Google login, register, OTP, refresh, logout, password reset |
| `ProfileController` | `/api/v1/me` | Current user profile CRUD, avatar, password change |
| `UsersController` | `/api/v1/users` | Admin: list users, activate/deactivate, assign roles |
| `PermissionManagementController` | `/api/v1/permission-management` | Role-permission matrix management |
| `StaffInvitationsController` | `/api/v1/staff-invitations` | Send/validate/accept invitations |
| `CategoriesController` | `/api/v1/categories` | Category CRUD |
| `CoursesController` | `/api/v1/courses` | Create/publish course, add lessons, reviews, bookmarks (no update/delete endpoints) |
| `EnrollmentsController` | `/api/v1/enrollments` | Enroll, my courses, lesson progress |
| `InstructorProfilesController` | `/api/instructor-profiles` | Instructor CRUD, approval, reviews, availability |
| `WalletsController` | `/api/wallets` | Balance, transaction history |
| `SessionsController` | `/api/v1/sessions` | Session CRUD, scheduling, instructor dashboard |
| `SessionMaterialsController` | `/api/sessions/{id}/materials` | Upload/download/reorder materials |
| `BookingsController` | `/api/v1/bookings` | Create/cancel/change status, my bookings, session bookings |
| `ChatController` | `/api/v1/chat` | Conversations, messages, mark read |
| `SupportController` | `/api/support` | Public FAQ search, contact form, my questions, email answer |
| `SupportManagementController` | `/api/support/manage` | Staff support queue: answer, publish, update, delete |
| `NotificationsController` | `/api/v1/notifications` | My notifications, unread count, mark read |
| `SiteSettingsController` | `/api/SiteSettings` | Get/update site settings (no route attribute — inherited from `BaseApiController`; update requires `SuperAdmin`) |
| `DevController` | `/api/v1/dev` | Development-only helpers — removed from the routing table and OpenAPI document outside Development |
| `TestFilesController` | `/api/test/files` | Local file storage test endpoints |

#### SignalR (`Hubs/`)
- `ChatHub` at `/hubs/chat` — Real-time 1-on-1 messaging
- `SignalRChatNotifier` — Implements `IChatNotifier` for server→client push
- `ChatUserIdProvider` — Maps JWT claims to SignalR connection

#### Middleware & Extensions

Middleware order in `PipelineExtensions.UseApplicationPipeline` is significant:

| Order | Component | Responsibility |
|---|---|---|
| 1 | `CorrelationIdMiddleware` | Validated trace ID — accepts a client value only if ≤ 64 chars and alphanumeric (`-`, `_`, `.`), otherwise generates a GUID. Prevents log forging via CR/LF and unbounded log growth. |
| 2 | `SecurityHeadersMiddleware` | `nosniff`, `X-Frame-Options: DENY`, CSP, `Referrer-Policy`; strips `X-Powered-By` |
| 3 | `RateLimitIdentityMiddleware` | Buffers and rewinds the JSON body to extract the target identity (email) for rate-limit partitioning. Must run **before** the limiter. |
| 4 | `UseRateLimiter` | Global per-IP limit plus per-endpoint policies partitioned by IP **and** identity |
| 5 | `UseExceptionHandler` / `GlobalExceptionHandler` | `UnauthorizedAccessException` → 401, `DbUpdateConcurrencyException` → 409, else ProblemDetails |
| 6 | `UseHsts` + `UseHttpsRedirection` | Transport security (HSTS outside Development) |
| 7 | `UseCors("AllowFrontend")` | Explicit origin allowlist; wildcard rejected at startup |
| 8–13 | Static files, OpenAPI, auth, controllers, SignalR hub | |

Other extensions:
- `ServiceCollectionExtensions` — DI wiring, Dev-controller removal convention, startup secret validation
- `SecurityConfigurationExtensions` — Fail-fast validation of `Jwt:Key`, `OtpSettings:HashingSecret`, OTP cooldown, CORS allowlist, connection strings. **The application does not start if any of these are unsafe.**
- `RateLimitingExtensions` — `login`, `otp-verify`, `otp-resend`, `email-test`, `contact-form`, `refresh-token` policies
- `ResultExtensions` — Maps `ErrorType` → HTTP status (Validation→400, NotFound→404, etc.)
- `LoggingExtensions` — Serilog configuration (Console + File + Enrichers)

#### Base controller

`BaseApiController.RequireUserId()` resolves the authenticated user id or throws
`UnauthorizedAccessException` (→ 401). This replaces the previous
`_currentUser.UserId ?? Guid.Empty` pattern used across eight controllers, where `Guid.Empty`
— a real, non-null value — flowed into commands and handlers ran queries and writes against a
non-existent user.

#### Contracts (`Contracts/`)
Request DTOs per feature (e.g., `CreateCourseRequest`, `EnrollInCourseRequest`, `CreateBookingRequest`) — sealed records, used only in controllers

---

## 🔐 Security Model

Full detail, including the secret-rotation runbook, lives in **[SECURITY.md](SECURITY.md)**.

| Control | Implementation |
|---|---|
| **Token signing** | HMAC-SHA256 with a validated ≥ 32-byte key. `exp`/`nbf`/`iat` use `DateTime.UtcNow`, never display-local time (the Egypt-local clock previously shifted every token's real lifetime by the UTC offset, plus an hour during DST). |
| **Refresh tokens** | Stored only as a SHA-256 digest. Rotation links tokens into a family; replaying a rotated token revokes that family (`TOKEN_REUSE_DETECTED`). Deactivated or unconfirmed accounts are rejected on refresh. |
| **Session lifecycle** | Password reset and account deactivation both revoke every refresh token for the user. Without this, an attacker holding a stolen token could reset the victim's password and stay authenticated. |
| **Rate limiting** | Global 300/min per IP, plus targeted policies. Partitions combine IP and target identity so lockout cannot be weaponised against a chosen victim. |
| **OTP** | HMAC-hashed with a purpose-derived key, so a code issued for email confirmation cannot be replayed for password reset. Cooldown and max-attempt limits per identifier. |
| **Authorization** | `[Permission]` on commands, plus role gates on admin controllers. `AdminEmailsController` requires `Admin`/`SuperAdmin`; its test-email recipient is restricted to the configured support address. |
| **Secrets** | No secret is committed. `appsettings.json` ships empty and the app refuses to start without valid values, rejecting known previously-committed placeholders. |
| **Bootstrap admin** | Opt-in via `Seed:SuperAdmin:*`. No default password exists in code; the seed is skipped with an error log when unconfigured. |
| **Transport** | HSTS, HTTPS redirection, security headers, strict CORS allowlist. |
| **Auditability** | Every request carries a validated correlation ID into Serilog's `LogContext` and error responses. |

---

## 🔄 Key Technical Flows

### 1. Atomic Enrollment (Course)
```
POST /api/v1/enrollments/enroll
  → EnrollInCourseCommand
  → Handler: Check wallet balance → UserWallet.DeductCredits() → Enrollment.Create()
  → Raises CourseEnrolledDomainEvent
  → Outbox → CreditInstructorWalletEventHandler → Instructor wallet + stats
```

### 2. Live Session Booking
```
GET /api/v1/sessions?bookableOnly=true → Lists sessions with AvailableSlots
POST /api/v1/bookings
  → CreateBookingCommand
  → Handler: Validate session (published, future, capacity, not self, no duplicate)
  → UserWallet.DeductCredits() → Booking.Create() → BookingCreatedDomainEvent
  → Outbox → (future: confirmation email)
PATCH /api/v1/bookings/{id}/status?status=Completed
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
| `Courses` | Id, Title, Description, Credits (from the `CoursePrice` owned type), Level, Status, InstructorId, CategoryId, AverageRating (`float(3)`), TotalReviews, IsDeleted |
| `CourseSections` | Id, CourseId, Title, OrderIndex |
| `CourseLessons` | Id, SectionId, Title, OrderIndex, IsPreviewable (attachments/videos live in owned collections) |
| `Enrollments` | Id, UserId, CourseId, CreditsPaid, Status, ProgressPercentage, LastWatchedLessonId |
| `UserWallets` | Id, UserId (UK), Balance, RowVersion (concurrency) |
| `WalletTransactions` | Id, WalletId, Amount, Type (CreditDeduction/CreditRefund/CreditReward), ReferenceId, Description, OccurredAt |
| `Sessions` | Id, InstructorId, Title, Description, ScheduledAtUtc, DurationMinutes, CreditsPrice, LocationType, LocationDetails, MaxParticipants, Status |
| `Bookings` | Id, SessionId, LearnerUserId, Status, PriceInCredits, ScheduledAtUtc, BookedAtUtc, StartedAtUtc, CompletedAtUtc, CancelledAtUtc, CancellationReason |
| `SessionMaterials` | Id, SessionId, FileName, DriveFileId, DriveFolderId, MimeType, SizeBytes, MaterialType, SortOrder, UploadedByUserId |
| `Conversations` | Id, ParticipantOneId, ParticipantTwoId, LastMessageAt, LastMessagePreview |
| `ChatMessages` | Id, ConversationId, SenderId, Content, SentAt, ReadAt |
| `Notifications` | Id, UserId, Type, Title, Body, IsRead, ReadAt, CreatedAt |
| `SupportQuestions` | Id, Question, Answer, Category, IsPublished, IsAnswered, AnsweredAt, UserEmail, UserName, AskedByUserId, EmailSent, EmailSentAt |
| `InstructorProfiles` | Id, UserId (1-1), Headline, Bio, IsApproved, Rating, SessionsCompleted, CreditsEarned |
| `InstructorReviews` | Id, InstructorProfileId, LearnerUserId, Rating, Comment, IsDeleted |
| `InstructorAvailabilities` | Id, InstructorProfileId, DayOfWeek, StartTime, EndTime |
| `TbPermissions` | Id, Name (e.g., "Categories.Manage"), Module |
| `TbRolePermissions` | RoleId, PermissionId, GrantedAt |
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
| Caching | Redis (StackExchange.Redis) + MemoryCacheService (⚠️ الـ fallback معطّل في `AddCaching.cs:40`) |
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
| Courses (Create/Publish/Lessons, Search, Cache) | ✅ Implemented — no update/delete endpoints |
| Enrollments (Atomic + Progress) | ✅ Implemented |
| Instructor Profiles & Reviews | ✅ Implemented |
| Instructor Availability | ✅ Implemented |
| Wallets (Balance, History, Earning, Refunds) | ✅ Partial — No Buy Credits / Promo Codes |
| Sessions (Scheduling, Bookability) | ✅ Implemented |
| Bookings (Full Lifecycle + Payment) | ✅ Implemented |
| Session Materials (Google Drive) | ✅ Implemented |
| Chat (REST + SignalR) | ✅ Implemented |
| Reviews (Course + Instructor) | ✅ Implemented — **No Session Reviews** |
| Background Jobs / Outbox | ✅ Implemented |
| Redis Caching | ⚠️ Implemented with startup bug — MemoryCache fallback commented out in `AddCaching.cs` |

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
- Unit test coverage gaps: Wallet (buy/promo), **Notifications**, and `GetMySessionsPaged` (Chat, OTP, Support, Categories, Site Settings, file storage are covered)
- Fix Redis fallback registration in `AddCaching.cs`
- Schedule `RefreshDriveQuotaJob` (registered in DI but never scheduled) or remove it
- Expose `CompleteBookingCommand` via `BookingsController` (command + tests exist, no endpoint)
- Mobile auth: Apple, Facebook providers
- Push notifications (FCM) for offline chat/booking alerts

---

## 🚀 Getting Started

> ⚠️ **No secret goes in `appsettings.json`.** The file is tracked in Git and ships with
> empty secret values. The application refuses to start if a required secret is missing, too
> short, or matches a previously-committed value. See **[SECURITY.md](SECURITY.md)**.

```bash
# 1. Supply secrets via user-secrets (never appsettings.json)
cd src/Skill-Loop.Api
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key"                   "$(openssl rand -base64 48)"
dotnet user-secrets set "OtpSettings:HashingSecret" "$(openssl rand -base64 48)"
dotnet user-secrets set "MailSettings:Password"     "your-app-password"
# Optional: create the first SuperAdmin (no default password exists)
dotnet user-secrets set "Seed:SuperAdmin:Email"    "admin@your-domain.com"
dotnet user-secrets set "Seed:SuperAdmin:Password" "$(openssl rand -base64 24)Aa1!"

# 2. Set the non-secret connection strings in appsettings.json.
#    HangfireConnection MUST be a separate database.

# 3. Apply migrations
dotnet ef database update --project ../Skill-Loop.Infrastructure

# 4. Run
dotnet run --project src/Skill-Loop.Api

# API:    https://localhost:7271
# Docs:   https://localhost:7271/scalar/v1
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