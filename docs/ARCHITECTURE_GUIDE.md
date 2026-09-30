# SkillLoop Backend — Architecture Guide

> **Purpose**: دليل شامل لأي مطوّر جديد يشرح كيف يبني Feature كاملة في هذا الـ Codebase.
> كل قاعدة مكتوبة هنا مأخوذة من الكود الفعلي، والمسارات مرفقة.
>
> **آخر تحديث**: 2026-09-26

---

## Table of Contents

1. [Solution Structure](#1-solution-structure)
2. [Domain Layer](#2-domain-layer)
   - 2.1 [Result Pattern](#21-result-pattern)
   - 2.2 [Entity Hierarchy](#22-entity-hierarchy)
   - 2.3 [Value Objects](#23-value-objects)
   - 2.4 [Enums](#24-enums)
   - 2.5 [Domain Events](#25-domain-events)
   - 2.6 [Constants (Roles & Permissions)](#26-constants-roles--permissions)
3. [Application Layer](#3-application-layer)
   - 3.1 [CQRS Messaging Abstractions](#31-cqrs-messaging-abstractions)
   - 3.2 [Pipeline Behaviors (MediatR)](#32-pipeline-behaviors-mediatr)
   - 3.3 [Error Convention](#33-error-convention)
   - 3.4 [Pagination](#34-pagination)
   - 3.5 [Caching Interfaces](#35-caching-interfaces)
   - 3.6 [Feature Folder Structure](#36-feature-folder-structure)
   - 3.7 [External Service Abstractions](#37-external-service-abstractions)
4. [Infrastructure Layer](#4-infrastructure-layer)
   - 4.1 [Persistence (EF Core + SQL Server)](#41-persistence-ef-core--sql-server)
   - 4.2 [Interceptors](#42-interceptors)
   - 4.3 [Outbox Pattern & Hangfire](#43-outbox-pattern--hangfire)
   - 4.4 [Dependency Injection](#44-dependency-injection)
5. [API Layer](#5-api-layer)
   - 5.1 [BaseApiController](#51-baseapicontroller)
   - 5.2 [ResultExtensions (ErrorType → HTTP)](#52-resultextensions-errortype--http)
   - 5.3 [Contracts (Request DTOs)](#53-contracts-request-dtos)
   - 5.4 [Controller Pattern](#54-controller-pattern)
   - 5.5 [Global Exception Handler](#55-global-exception-handler)
   - 5.6 [Pipeline & Routing](#56-pipeline--routing)
   - 5.7 [SignalR (Real-time Chat)](#57-signalr-real-time-chat)
6. [Unit Tests](#6-unit-tests)
7. [End-to-End Checklist — Building a New Feature](#7-end-to-end-checklist--building-a-new-feature)
   - 7.1 [New Command](#71-new-command)
   - 7.2 [New Query](#72-new-query)
   - 7.3 [New Entity](#73-new-entity)
   - 7.4 [New Domain Event + Handler](#74-new-domain-event--handler)
   - 7.5 [New External Service](#75-new-external-service)
   - 7.6 [New Hangfire Job](#76-new-hangfire-job)
   - 7.7 [New Permission](#77-new-permission)
8. [Inconsistencies & Risks](#8-inconsistencies--risks)
9. [Module Gap Analysis (Target vs Current)](#9-module-gap-analysis-target-vs-current)

---

## 1. Solution Structure

```
Skill-Loop/
├── src/
│   ├── Skill-Loop.Domain/           ← الطبقة الداخلية: الكيانات، القيم، الأحداث، الثوابت
│   ├── Skill-Loop.Application/      ← منطق الأعمال: Commands, Queries, Behaviors, DTOs
│   ├── Skill-Loop.Infrastructure/   ← التطبيقات الخارجية: EF Core, Hangfire, Email, Cache, Google Drive
│   └── Skill-Loop.Api/              ← نقطة الدخول: Controllers, Contracts, Middleware, SignalR Hubs
├── tests/
│   └── Skill-Loop.UnitTests/        ← اختبارات الوحدة
├── docs/                            ← هذا الملف
└── Skill-Loop.slnx                  ← ملف الحل (Solution)
```

**Dependency flow**: `Api → Application → Domain` ← `Infrastructure`

> Infrastructure تعتمد على Application (لتطبيق الـ Abstractions) لكن Api تعتمد على Infrastructure فقط لتسجيل الـ DI.

---

## 2. Domain Layer

### 2.1 Result Pattern

**Files**:

- `src/Skill-Loop.Domain/Common/Results/Result.cs`
- `src/Skill-Loop.Domain/Common/Results/Error.cs`
- `src/Skill-Loop.Domain/Common/Results/ErrorType.cs`

#### Error Record

```csharp
// Error.cs
public record Error(string Code, string Description, ErrorType Type)
{
    public static Error None => new(string.Empty, string.Empty, ErrorType.Failure);
}
```

#### ErrorType Enum → HTTP Status Code Mapping

```csharp
// ErrorType.cs
public enum ErrorType
{
    Failure = 0,       // 400 Bad Request (default)
    Validation = 1,    // 400 Bad Request
    NotFound = 2,      // 404 Not Found
    Unauthorized = 3,  // 401 Unauthorized
    Forbidden = 4,     // 403 Forbidden
    Conflict = 5,      // 409 Conflict
    Unexpected = 6     // 500 Internal Server Error
}
```

> الربط الفعلي مع HTTP يتم في `src/Skill-Loop.Api/Extensions/ResultExtensions.cs` (انظر [قسم 5.2](#52-resultextensions-errortype--http)).

#### Result Class — Factory Methods

| Method                                | Return Type | Usage                                               |
| ------------------------------------- | ----------- | --------------------------------------------------- |
| `Result.Success(message?)`            | `Result`    | نجاح بدون بيانات                                    |
| `Result.Failure(Error error)`         | `Result`    | فشل بخطأ واحد                                       |
| `Result.Failure(IEnumerable<Error>)`  | `Result`    | فشل بعدة أخطاء                                      |
| `Result.Failure(string)`              | `Result`    | فشل برسالة (يُحوّل لـ Error مع `ErrorType.Failure`) |
| `Result<T>.Success(T data, message?)` | `Result<T>` | نجاح مع بيانات                                      |
| `Result<T>.Failure(Error error)`      | `Result<T>` | فشل بخطأ واحد                                       |
| `Result<T>.Failure(IEnumerable<Error>)` | `Result<T>` | فشل بعدة أخطاء (`Result.cs:86`)                  |

**Properties**: `Succeeded`, `IsSuccess`, `IsFailure`, `Message`, `Errors`, `Data` (في `Result<T>`).

---

### 2.2 Entity Hierarchy

**Files**: `src/Skill-Loop.Domain/Common/Entities/`

```
Entity (abstract)                   ← يدعم Domain Events فقط
  └── BaseEntity (abstract)         ← يضيف Id: Guid (V7)
       └── AuditableEntity (abstract) ← يضيف CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
            └── SoftDeleteEntity (abstract) ← يضيف IsDeleted, DeletedAt, DeletedBy
```

| Base Class         | متى تستخدمه                        | Id  | Audit | Soft Delete |
| ------------------ | ---------------------------------- | --- | ----- | ----------- |
| `Entity`           | كيان بدون Id خاص (نادر)            | ❌  | ❌    | ❌          |
| `BaseEntity`       | كيان بسيط بـ Guid V7               | ✅  | ❌    | ❌          |
| `AuditableEntity`  | كيان يحتاج تتبع الإنشاء والتعديل   | ✅  | ✅    | ❌          |
| `SoftDeleteEntity` | كيان يحتاج حذف منطقي (soft delete) | ✅  | ✅    | ✅          |

**ملاحظة**: الـ `Id` يستخدم `Guid.CreateVersion7()` — وهو GUID مرتب زمنياً.

```csharp
// BaseEntity.cs
public abstract class BaseEntity : Entity
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
}
```

#### الكيانات الحالية في المشروع

```
Domain/Entities/
├── Booking/
│   ├── Booking.cs                    ← AuditableEntity (حجز جلسة)
│   ├── BookingDomainErrors.cs        ← أخطاء الـ Domain للـ Booking
│   └── Events/
│       └── BookingDomainEvents.cs    ← BookingCreated / BookingCancelled
├── Chat/
│   ├── Conversation.cs               ← AuditableEntity (محادثة 1-لـ-1)
│   ├── ChatMessage.cs                ← BaseEntity (رسالة واحدة)
│   └── Events/
│       └── ChatMessageSentEvent.cs
├── Courses/
│   ├── Course.cs                     ← SoftDeleteEntity (الكورس الرئيسي — Aggregate Root)
│   ├── Category.cs                   ← AuditableEntity (التصنيف)
│   ├── Section.cs                    ← BaseEntity (قسم داخل الكورس)
│   ├── Lesson.cs                     ← BaseEntity (درس داخل القسم)
│   ├── CourseReview.cs               ← AuditableEntity (تقييم الكورس)
│   ├── CourseBookmark.cs             ← SoftDeleteEntity (إشارة مرجعية)
│   ├── Events/
│   │   └── CourseDomainEvents.cs
│   └── ValueObjects/
│       ├── CoursePrice.cs            ← Value Object (سعر بالكريديت)
│       ├── CourseRating.cs           ← Value Object (متوسط التقييم)
│       ├── VideoResource.cs          ← Value Object (رابط فيديو + مدة)
│       └── PdfAttachment.cs          ← Value Object (مرفق PDF)
├── Enrollments/
│   ├── Enrollment.cs                 ← AuditableEntity (تسجيل في كورس)
│   ├── LessonProgress.cs             ← BaseEntity (تقدم الطالب في درس)
│   └── Events/
│       └── EnrollmentDomainEvents.cs
├── Instructors/
│   ├── InstructorProfile.cs          ← AuditableEntity (بروفايل المدرب — Aggregate Root)
│   ├── InstructorReview.cs           ← SoftDeleteEntity (تقييم المدرب من الطلاب)
│   └── InstructorAvailability.cs     ← BaseEntity (مواعيد توفر المدرب)
├── Invitation/
│   ├── StaffInvitation.cs            ← AuditableEntity (دعوة موظف)
│   └── Events/
│       └── StaffInvitationCreatedEvent.cs
├── Notifications/
│   └── Notification.cs               ← AuditableEntity (إشعار جوه التطبيق)
├── OtpVerification/
│   └── OtpVerification.cs            ← BaseEntity (كود التحقق)
├── Session/
│   ├── Session.cs                    ← AuditableEntity (جلسة تعليمية 1-لـ-1)
│   ├── SessionMaterial.cs            ← AuditableEntity (ملف مرفق بالجلسة)
│   └── Events/
│       ├── SessionMaterialUploadedEvent.cs
│       └── SessionCompletedDomainEvent.cs
├── SiteSettings/
│   └── SiteSettings.cs              ← BaseEntity (إعدادات الموقع)
├── Support/
│   └── SupportQuestion.cs            ← AuditableEntity (سؤال FAQ / استفسار دعم)
└── Wallets/
    ├── UserWallet.cs                 ← AuditableEntity (محفظة المستخدم)
    ├── WalletTransaction.cs          ← BaseEntity (حركة مالية)
    └── Events/
        └── WalletDomainEvents.cs
```

#### InstructorProfile — بروفايل المدرب (Aggregate Root)

**Files**: `src/Skill-Loop.Domain/Entities/Instructors/`

`InstructorProfile` هو Aggregate Root يمثل الملف الشخصي للمدرب ويتضمن:
- علاقة 1-to-1 مع `ApplicationUser` عبر `UserId`
- بيانات العرض: `Headline`, `Bio`
- حالة الاعتماد: `IsApproved` (يبدأ `false` — يحتاج موافقة الإدارة)
- إحصائيات: `Rating`, `SessionsCompleted`, `CreditsEarned`
- مجموعة المراجعات: `Reviews` (1-لـ-متعدد مع `InstructorReview`)

```csharp
public sealed class InstructorProfile : AuditableEntity
{
    public Guid UserId { get; private set; }
    public string Headline { get; private set; } = string.Empty;
    public string Bio { get; private set; } = string.Empty;
    public bool IsApproved { get; private set; }
    public double Rating { get; private set; }
    public int SessionsCompleted { get; private set; }
    public int CreditsEarned { get; private set; }
    public IReadOnlyCollection<InstructorReview> Reviews => _reviews.AsReadOnly();
    public IReadOnlyCollection<InstructorAvailability> Availabilities => _availabilities.AsReadOnly();

    private InstructorProfile() { }

    public static Result<InstructorProfile> Create(Guid userId, string headline, string bio) { ... }
    public Result UpdateDetails(string headline, string bio) { ... }
    public void Approve() => IsApproved = true;
    public void Suspend() => IsApproved = false;
    public void IncrementSessionsCompleted() => SessionsCompleted++;
    public void AddCreditsEarned(int amount) => CreditsEarned += amount;
    public Result AddReview(Guid learnerUserId, int rating, string? comment) { ... }
    public Result UpdateReview(Guid reviewId, Guid learnerUserId, int rating, string? comment) { ... }
    public Result RemoveReview(Guid reviewId, Guid learnerUserId) { ... }
    public Result AddAvailability(DayOfWeek day, TimeSpan start, TimeSpan end) { ... }
    public Result RemoveAvailability(Guid availabilityId) { ... }
    private void RecalculateRating() { ... } // حساب متوسط التقييم تلقائياً
}
```

#### InstructorReview — تقييم المدرب

```csharp
public sealed class InstructorReview : SoftDeleteEntity
{
    public Guid InstructorProfileId { get; private set; }
    public Guid LearnerUserId { get; private set; }
    public int Rating { get; private set; } // 1 إلى 5
    public string Comment { get; private set; } = string.Empty;

    public static Result<InstructorReview> Create(Guid instructorProfileId, Guid learnerUserId, int rating, string? comment) { ... }
    public void Update(int rating, string? comment) { ... }
}
```

---

### 2.3 Value Objects

**Files**: `src/Skill-Loop.Domain/Entities/Courses/ValueObjects/`

| Value Object    | الوصف                                                              | Factory Method                                    |
| --------------- | ------------------------------------------------------------------ | ------------------------------------------------- |
| `CoursePrice`   | سعر الكورس بالكريديت. `IsFree` = true لو `Credits == 0`            | `Create(int credits)` مع validation + `Free()`    |
| `CourseRating`  | متوسط التقييم وعدد المراجعات. `AddRating(int)` يحسب المتوسط الجديد | `Empty()` + `Create(double, int)`                 |
| `VideoResource` | رابط الفيديو + المدة + الجودة + معرّف المزوّد الخارجي              | `Create(url, duration, resolution?, providerId?)` |
| `PdfAttachment` | اسم الملف + رابط التخزين + الحجم                                   | `Create(fileName, storageUrl, fileSizeBytes)`     |

**مثال — CoursePrice**:

```csharp
public sealed record CoursePrice
{
    public int Credits { get; private init; }
    public bool IsFree => Credits == 0;

    public static Result<CoursePrice> Create(int credits)
    {
        if (credits < 0)
            return Result<CoursePrice>.Failure(new Error("CoursePrice.Negative", "Course credits cannot be negative.", ErrorType.Validation));
        return Result<CoursePrice>.Success(new CoursePrice(credits));
    }

    public static CoursePrice Free() => new(0);
}
```

---

### 2.4 Enums

**Files**: `src/Skill-Loop.Domain/Enums/`

| Enum               | القيم                                               | الاستخدام                     |
| ------------------ | --------------------------------------------------- | ----------------------------- |
| `CourseLevel`      | `Beginner`, `Intermediate`, `Advanced`, `AllLevels` | مستوى الكورس                  |
| `CourseStatus`     | `Draft`, `Published`, `Archived`                    | حالة نشر الكورس               |
| `EnrollmentStatus` | `Active`, `Completed`, `Cancelled`                  | حالة التسجيل في الكورس        |
| `TransactionType`  | `CreditDeduction`, `CreditRefund`, `CreditReward`   | نوع الحركة المالية في المحفظة |
| `SessionStatus`    | `Draft`, `Published`, `Completed`, `Cancelled`    | حالة الجلسة                   |
| `SessionType`      | `Online`, `Offline`                                | نوع الجلسة                     |
| `SessionLocationType` | `Online`, `Offline`                             | مكان الجلسة (أونلاين/أوفلاين)  |
| `BookingStatus`    | `Pending`, `Confirmed`, `InProgress`, `Completed`, `Cancelled`, `Rejected`, `NoShow` | حالة حجز الجلسة |
| `MaterialType`     | `PDF`, `Video`, `Image`, `Document`, `Other`        | نوع الملف المرفق بالجلسة      |
| `OtpPurpose`       | `VerifyPhone`, `EmailVerification`, `PasswordReset` | الغرض من كود التحقق           |

---

### 2.5 Domain Events

**Files**:

- `src/Skill-Loop.Domain/Common/Events/IDomainEvent.cs` — واجهة فارغة (marker)
- `src/Skill-Loop.Domain/Common/Events/IHasDomainEvents.cs` — عقد للكيانات التي ترفع أحداث

**الآلية**:

1. الكيان يرفع حدث عبر `AddDomainEvent()` (متاحة من `Entity`)
2. يتم جمع الأحداث عبر `InsertOutboxMessagesInterceptor` قبل `SaveChanges` ← تُخزّن في جدول `OutboxMessages`
3. `ProcessOutboxMessagesJob` (Hangfire) يقرأ الـ Outbox ← يغلّف الحدث في `DomainEventNotification<T>` ← ينشره عبر `IPublisher.Publish()`

#### الأحداث الموجودة حالياً

| Module         | Event                                                                                               | ملف المصدر                                                   |
| -------------- | --------------------------------------------------------------------------------------------------- | ------------------------------------------------------------ |
| **Course**     | `CourseCreatedDomainEvent(Guid CourseId, string Title)`                                             | `Entities/Courses/Events/CourseDomainEvents.cs`              |
| **Course**     | `CourseUpdatedDomainEvent(Guid CourseId, string Title)`                                             | `Entities/Courses/Events/CourseDomainEvents.cs`              |
| **Course**     | `CoursePublishedDomainEvent(Guid CourseId, string Title)`                                           | `Entities/Courses/Events/CourseDomainEvents.cs`              |
| **Enrollment** | `CourseEnrolledDomainEvent(Guid EnrollmentId, Guid UserId, Guid CourseId, int CreditsPaid)`         | `Entities/Enrollments/Events/EnrollmentDomainEvents.cs`      |
| **Enrollment** | `LessonCompletedDomainEvent(Guid EnrollmentId, Guid UserId, Guid CourseId, Guid LessonId)`          | `Entities/Enrollments/Events/EnrollmentDomainEvents.cs`      |
| **Enrollment** | `CourseCompletedDomainEvent(Guid EnrollmentId, Guid UserId, Guid CourseId)`                         | `Entities/Enrollments/Events/EnrollmentDomainEvents.cs`      |
| **Wallet**     | `WalletBalanceDeductedDomainEvent(Guid UserId, int AmountDeducted, int RemainingBalance)`           | `Entities/Wallets/Events/WalletDomainEvents.cs`              |
| **Wallet**     | `WalletBalanceRefundedDomainEvent(Guid UserId, int AmountRefunded, int RemainingBalance)`          | `Entities/Wallets/Events/WalletDomainEvents.cs`              |
| **Booking**    | `BookingCreatedDomainEvent(...)` / `BookingCancelledDomainEvent(...)`                                | `Entities/Booking/Events/BookingDomainEvents.cs`              |
| **Chat**       | `ChatMessageSentEvent(Guid MessageId, Guid ConversationId, Guid SenderId)`                           | `Entities/Chat/Events/ChatMessageSentEvent.cs`                 |
| **Session**    | `SessionMaterialUploadedEvent(SessionMaterial Material)`                                            | `Entities/Session/Events/SessionMaterialUploadedEvent.cs`    |
| **Session**    | `SessionCompletedDomainEvent(Guid SessionId, Guid InstructorId, Guid LearnerUserId, int PriceInCredits)` | `Entities/Session/Events/SessionCompletedDomainEvent.cs` |
| **Invitation** | `StaffInvitationCreatedEvent(StaffInvitation Invitation)`                                           | `Entities/Invitation/Events/StaffInvitationCreatedEvent.cs`  |

**مثال كامل — Course يرفع حدث**:

```csharp
// src/Skill-Loop.Domain/Entities/Courses/Events/CourseDomainEvents.cs
public sealed record CourseCreatedDomainEvent(Guid CourseId, string Title) : IDomainEvent;

// src/Skill-Loop.Domain/Entities/Courses/Course.cs
public static Result<Course> Create(...)
{
    // ... validation ...
    var course = new Course { ... };
    course.AddDomainEvent(new CourseCreatedDomainEvent(course.Id, course.Title));
    return Result<Course>.Success(course);
}
```

---

### 2.6 Constants (Roles & Permissions)

**Files**:

- `src/Skill-Loop.Domain/Constants/Roles.cs`
- `src/Skill-Loop.Domain/Constants/Permissions.cs`

#### Roles

```csharp
public static class Roles
{
    public const string SuperAdmin = "SuperAdmin";       // المدير العام
    public const string Admin = "Admin";                 // مدير المحتوى
    public const string FinanceManager = "FinanceManager"; // المسؤول المالي
    public const string Support = "Support";             // الدعم
    public const string User = "User";                   // المستخدم العادي (Learner)
    public const string Instructor = "Instructor";       // المعلم

    public static readonly IReadOnlyList<string> All = new[]
    { SuperAdmin, Admin, FinanceManager, Support, User, Instructor };

    // أدوار لوحة التحكم (Staff)
    public static readonly IReadOnlyList<string> StaffRoles = new[]
    { SuperAdmin, Admin, FinanceManager, Support };

    // أدوار مستخدمي الموبايل
    public static readonly IReadOnlyList<string> MobileRoles = new[]
    { User, Instructor };
}
```

#### Permissions

```csharp
public class Permissions
{
    public static class Catalog
    {
        public const string CategoriesManage = "Categories.Manage";
        public const string SkillsManage = "Skills.Manage";
        public const string TagsManage = "Tags.Manage";
        public const string SessionsModerate = "Sessions.Moderate";
    }

    public static class Users
    {
        public const string View = "Users.View";
        public const string Activate = "Users.Activate";
        public const string Deactivate = "Users.Deactivate";
        public const string AssignRole = "Users.AssignRole";
    }

    public static class Finance
    {
        public const string PackagesManage = "Packages.Manage";
        public const string PromoCodesManage = "PromoCodes.Manage";
        public const string WalletAdjust = "Wallet.Adjust";
        public const string PaymentsView = "Payments.View";
    }

    public static class Bookings
    {
        public const string View = "Bookings.View";
    }

    public static class Access
    {
        public const string RolesManage = "Roles.Manage";
        public const string PermissionsManage = "Permissions.Manage";
        public const string InvitationsSend = "Invitations.Send";
    }

    public static class Sessions
    {
        public const string Moderate = "Sessions.Moderate";
        public const string ViewMaterials = "Sessions.ViewMaterials";
        public const string UploadMaterials = "Sessions.UploadMaterials";
        public const string DeleteMaterials = "Sessions.DeleteMaterials";
        public const string ReorderMaterials = "Sessions.ReorderMaterials";
    }

    public static class Dashboards
    {
        public const string ViewAdmin = "Dashboards.ViewAdmin";
        public const string ViewInstructor = "Dashboards.ViewInstructor";
    }

    // دالة Reflection لجلب جميع الصلاحيات تلقائياً
    public static IReadOnlyList<string> GetAllPermissions() { ... }
}
```

> **طريقة إضافة صلاحية جديدة**: أضف `nested static class` بداخل `Permissions` وأضف `const string` بداخلها. الـ Seed يلتقطها تلقائياً عبر Reflection.

**Seeding** (`src/Skill-Loop.Infrastructure/Persistence/Seed/ContextSeed.cs`, driven by `AddDatabaseSeeder.cs`):

1. `MigrateAsync()` — يطبّق الـ migrations المعلقة
2. ينشئ الأدوار من `Roles.All`
3. يجلب كل الصلاحيات من `Permissions.GetAllPermissions()` ويضيف الجديد لجدول `TbPermissions`
4. يربط كل الصلاحيات بدور `SuperAdmin` عبر جدول `TbRolePermissions`
5. ينشئ حساب الـ SuperAdmin و `SeedSiteSettingsAsync` لصف إعدادات الموقع

> كل ده بيشتغل في `app.SeedDatabaseAsync()` **قبل** بناء الـ HTTP pipeline (`Program.cs`).

---

## 3. Application Layer

### 3.1 CQRS Messaging Abstractions

**Files**: `src/Skill-Loop.Application/Common/Abstractions/Messaging/`

```csharp
// ICommand.cs — أمر بدون بيانات مرتجعة
public interface ICommand : IRequest<Result> { }

// ICommand.cs — أمر مع بيانات مرتجعة
public interface ICommand<TResponse> : IRequest<Result<TResponse>> { }

// IQuery.cs — استعلام (دائماً يرجع بيانات)
public interface IQuery<TResponse> : IRequest<Result<TResponse>> { }
```

**Handlers**:

```csharp
// ICommandHandler.cs
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand { }

public interface ICommandHandler<in TCommand, TResponse>
    : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse> { }

// IQueryHandler.cs
public interface IQueryHandler<in TQuery, TResponse>
    : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse> { }
```

---

### 3.2 Pipeline Behaviors (MediatR)

**File**: `src/Skill-Loop.Application/DependencyInjection.cs`

الترتيب المسجّل (مهم! يُنفّذ من الأول للآخر):

| #   | Behavior                    | مهمته                                                   | يُفعّل على                                         |
| --- | --------------------------- | ------------------------------------------------------- | -------------------------------------------------- |
| 1   | `LoggingBehavior`           | يسجل بداية/نهاية كل Request مع CorrelationId            | كل Request                                         |
| 2   | `PerformanceBehavior`       | يحذر إذا تجاوز الطلب 800ms                              | كل Request                                         |
| 3   | `AuthorizationBehavior`     | يفحص `[Permission]` attribute ← يتحقق من `ICurrentUser` | فقط إذا وُجد `[PermissionAttribute]`               |
| 4   | `ValidationBehavior`        | يجمع أخطاء FluentValidation ← يرجع `Result.Failure`     | إذا وُجد `IValidator<TRequest>`                    |
| 5   | `CachingBehavior`           | يقرأ من الكاش (GetOrCreateAsync)                        | فقط `ICacheableQuery`                              |
| 6   | `CacheInvalidationBehavior` | يمسح الكاش بالـ prefix بعد النجاح                       | فقط `ICacheInvalidatorCommand`                     |
| 7   | `TransactionBehavior`       | يلف الـ Handler في DB transaction                       | فقط `ICommand<TResponse> where TResponse : Result` |

> **ملاحظة هامة**: `TransactionBehavior` عنده constraint: `where TRequest : ICommand<TResponse>` و `where TResponse : Result`. هذا يعني إنه **لا يُفعّل** على `ICommand` (بدون TResponse) ولا على الـ Queries!

#### PermissionAttribute

```csharp
// src/Skill-Loop.Application/Common/Abstractions/Identity/Authorization/PermissionAttribute.cs
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class PermissionAttribute : Attribute
{
    public string Name { get; }
    public PermissionAttribute(string name) { Name = name; }
}
```

**الاستخدام**: تزيّن الـ Command/Query class:

```csharp
[Permission("Categories.Manage")]
public sealed record CreateCategoryCommand(...) : ICommand<Guid>;
```

#### ResultFactory Helper

```csharp
// src/Skill-Loop.Application/Common/Helpers/ResultFactory.cs
public static class ResultFactory
{
    // يستخدم Reflection لإنشاء Result.Failure أو Result<T>.Failure
    // يُستخدم في Behaviors حيث TResponse generic
    public static TResponse CreateFailure<TResponse>(Error error) { ... }
}
```

---

### 3.3 Error Convention

**Files**: `src/Skill-Loop.Application/Common/Errors/`

```
Errors/
├── Bookings/
│   └── BookingErrors.cs
├── Chat/
│   └── ChatErrors.cs
├── Files/
│   └── FileErrors.cs
├── Identity/
│   ├── ExternalAuthErrors.cs
│   ├── OtpErrors.cs
│   ├── PasswordErrors.cs
│   ├── TokenErrors.cs
│   └── UserErrors.cs
├── Instructors/
│   └── InstructorProfileErrors.cs
├── Invitations/
│   └── InvitationErrors.cs
├── Notifications/
│   └── NotificationErrors.cs
└── Sessions/
    ├── SessionErrors.cs
    └── SessionMaterialErrors.cs
```

> Support أخطاءه مش هنا — هو بيستخدم `Result` مع أكواد `SupportQuestion.*` جوّه الـ Handlers والـ Domain مباشرة (شوف [Support_Subsystem.md](Support_Subsystem.md)).

**InstructorProfileErrors**:

```csharp
public static class InstructorProfileErrors
{
    public static readonly Error NotFound = new(
        "INSTRUCTOR_PROFILE_NOT_FOUND",
        "لم يتم العثور على الملف الشخصي للمدرب.",
        ErrorType.NotFound);

    public static readonly Error AlreadyExists = new(
        "INSTRUCTOR_PROFILE_ALREADY_EXISTS",
        "يوجد ملف شخصي لهذا المدرب بالفعل.",
        ErrorType.Conflict);
}
```

**Convention**:

- اسم الكلاس: `{Domain}Errors` (مثل `UserErrors`, `ChatErrors`, `SessionMaterialErrors`)
- الكلاس `static class`
- الأخطاء الثابتة: `public static readonly Error`
- الأخطاء الديناميكية: `public static Error MethodName(string details) => new(...)`
- كود الخطأ: `"DOMAIN_ERROR_CODE"` بحروف كبيرة وفواصل underscores
- الوصف: نص واضح — أغلبه بالإنجليزي (مثل `"User not found."`)، وبعضه بالعربي (زي أخطاء الدعم)

```csharp
// مثال من UserErrors.cs
public static class UserErrors
{
    // ثابت
    public static readonly Error NotFound = new(
        "USER_NOT_FOUND",
        "المستخدم غير موجود.",
        ErrorType.NotFound);

    // ديناميكي
    public static Error UpdateFailed(string details) => new(
        "USER_UPDATE_FAILED",
        $"فشل تحديث ملف المستخدم: {details}",
        ErrorType.Failure);
}
```

**لإضافة أخطاء لـ feature جديدة**: أنشئ مجلد بداخل `Errors/` واسم الكلاس `{Feature}Errors.cs`.

---

### 3.4 Pagination

**Files**:

- `src/Skill-Loop.Application/Common/Pagination/PaginationParameters.cs`
- `src/Skill-Loop.Application/Common/Pagination/PaginationMetadata.cs`
- `src/Skill-Loop.Application/Common/Pagination/PagedResult.cs`
- `src/Skill-Loop.Api/Contracts/Common/PaginationRequest.cs`

```csharp
// PaginationParameters.cs — تُستخدم داخل Application Layer
public class PaginationParameters
{
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 100;
    public int PageNumber { get; init; }  // min 1
    public int PageSize { get; init; }    // min 1, max 100
    public int Skip => (PageNumber - 1) * PageSize;
}

// PagedResult.cs — النتيجة المرتجعة
public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public PaginationMetadata Pagination { get; init; } = null!;
}

// PaginationRequest.cs — من Api layer (الـ Contract)
public record PaginationRequest
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
```

---

### 3.5 Caching Interfaces

**Files**: `src/Skill-Loop.Application/Common/Abstractions/External/Cache/`

```csharp
// ICacheableQuery.cs — للـ Queries التي نريد تخزينها مؤقتاً
public interface ICacheableQuery
{
    string CacheKey { get; }
    TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(5);
    TimeSpan? AbsoluteExpiration => TimeSpan.FromMinutes(30);
}

public interface ICacheableQuery<TResponse> : IQuery<TResponse>, ICacheableQuery { }

// ICacheInvalidatorCommand.cs — للـ Commands التي تمسح الكاش
public interface ICacheInvalidatorCommand
{
    IReadOnlyCollection<string> CacheKeys { get; }
}
```

---

### 3.6 Feature Folder Structure

كل Feature تتبع هذا النمط:

```
Features/
└── {FeatureName}/
    ├── Commands/
    │   └── {ActionName}/
    │       ├── {ActionName}Command.cs
    │       ├── {ActionName}CommandHandler.cs
    │       └── {ActionName}CommandValidator.cs
    ├── Queries/
    │   └── {QueryName}/
    │       ├── {QueryName}Query.cs
    │       ├── {QueryName}QueryHandler.cs
    │       └── {QueryName}QueryValidator.cs  (اختياري)
    ├── EventHandlers/
    │   └── {EventName}/
    │       └── {EventName}EventHandler.cs
    ├── DTOs/
    │   └── {Feature}Dto.cs  (DTOs مشتركة)
    └── Shared/  (أو Share/)
        └── {Feature}Response.cs  (اختياري)
```

> **انحراف مقصود موجود في الكود**: الميزات دي بتستخدم ملف واحد مدمج `{Action}.cs` (الـ Command + الـ Handler + الـ Validator في نفس الملف) بدل 3 ملفات منفصلة: `Enrollments`, `Courses`, `Chat`, `Notifications`, `Bookings`/`Support` جزئياً. كمان مجلد الـ DTOs المشتركة اسمه `Share/` في `Support` و `Instructors` و `Sessions/Queries`.

#### Features الموجودة حالياً

| Feature                | Commands                                                                                                                                                                                                                                                                                                                  | Queries                                                                                                          | EventHandlers                                   | DTOs |
| ---------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------- | ----------------------------------------------- | ---- |
| **Accounts**           | ChangePassword, ForgotPassword, ResetPassword, StaffLogin, StaffGoogleLogin, UserLogin, UserGoogleLogin, RegisterUser, VerifyEmailOtp, ResendEmailOtp, RefreshToken, Logout, SendInvitation, AcceptInvitation, AcceptInvitationWithGoogle, ActivateUser, DeactivateUser, AssignRoleToUser, RemoveRoleFromUser, AssignPermissionToRole, RemovePermissionFromRole, UpdateRolePermissions, UpdateMyProfile, UpdateMyProfilePicture | GetAllUsers, GetMyProfile, GetUserById, GetAllPermissions, GetRolePermissions, GetAllRolesWithPermissions, ValidateInvitation | StaffInvitationCreated                          | ✅   |
| **Categories**         | CreateCategory, UpdateCategory, DeleteCategory                                                                                                                                                                                                                                                                            | GetCategories                                                                                                    | —                                               | —    |
| **Courses**            | CreateCourse, AddLesson, AddCourseReview, PublishCourse, ToggleCourseBookmark                                                                                                                                                                                                                                             | GetCourseById, GetCoursesPaged                                                                                   | ✅ (CourseInvalidationHandler)                   | ✅   |
| **Enrollments**        | EnrollInCourse, UpdateLessonProgress                                                                                                                                                                                                                                                                                      | GetUserEnrolledCourses                                                                                           | —                                               | ✅   |
| **Instructors**        | CreateMyInstructorProfile, UpdateMyInstructorProfile, ChangeInstructorApprovalStatus, AddInstructorAvailability, RemoveInstructorAvailability, AddInstructorReview, UpdateInstructorReview, RemoveInstructorReview                                                                                    | GetInstructorsPaged, GetInstructorProfileByUserId, GetInstructorFullProfileByUserId                               | SessionCompleted, CourseEnrolled                | ✅   |
| **Wallets**            | — (no buy/promo commands yet)                                                                                                                                                                                                                                                                                             | GetMyWallet, GetMyWalletTransactionsPaged                                                                        | CourseEnrolled (CreditInstructorWallet)          | ✅   |
| **Sessions**           | CreateSession, UpdateSession, DeleteSession, ChangeSessionStatus                                                                                                                                                                                                                                                          | GetSessionById, GetSessionsPaged, GetMySessionsPaged                                                             | —                                               | ✅ (`Queries/Share/SessionResponse.cs`) |
| **Sessions/Materials** | UploadSessionMaterial, DeleteSessionMaterial, ReorderSessionMaterials                                                                                                                                                                                                                                                     | GetSessionMaterials, GetSessionMaterialDownloadInfo                                                              | SessionMaterialUploaded                         | —    |
| **Bookings**           | CreateBooking, CancelBooking, ChangeBookingStatus, CompleteBooking (بدون endpoint في الكنترولر)                                                                                                                                                                                                                            | GetBookingById, GetMyBookings, GetSessionBookings                                                                | —                                               | —    |
| **Chat**               | StartConversation, SendMessage, MarkConversationRead                                                                                                                                                                                                                                                                      | GetMyConversations, GetConversationMessages                                                                      | ChatMessageSent (ينشئ In-app Notification)      | ✅   |
| **Dashboards**         | —                                                                                                                                                                                                                                                                                                                         | GetAdminDashboardSummary, GetInstructorDashboardSummary, GetStudentDashboardSummary                              | —                                               | —    |
| **Notifications**      | MarkNotificationRead, MarkAllNotificationsRead                                                                                                                                                                                                                                                                             | GetMyNotifications, GetUnreadNotificationCount                                                                     | —                                               | ✅   |
| **Support**            | SubmitContactForm, SendFaqAnswerEmail, CreateSupportQuestion, UpdateSupportQuestion, AnswerSupportQuestion, SetSupportQuestionPublication, DeleteSupportQuestion                                                                                                                                                       | GetPublishedSupportQuestionsPaged, GetSupportQuestionById, GetSupportQuestionsPaged, GetSupportQuestionDetails, GetMySupportQuestions | —                              | ✅ (`Share/`) |
| **SiteSettings**       | UpdateSiteSettings                                                                                                                                                                                                                                                                                                        | GetSiteSettings                                                                                                  | —                                               | —    |

---

### 3.7 External Service Abstractions

**Files**: `src/Skill-Loop.Application/Common/Abstractions/External/`

| Category        | Interface                  | الوصف                                |
| --------------- | -------------------------- | ------------------------------------ |
| **Cache**       | `ICacheService`            | قراءة/كتابة الكاش                    |
| **Cache**       | `ICacheableQuery<T>`       | Query مع caching                     |
| **Cache**       | `ICacheInvalidatorCommand` | Command يمسح الكاش                   |
| **Client**      | `IClientContext`           | بيانات العميل (IP, User-Agent, etc.) — الواجهة في `Common/Abstractions/Web/` والتنفيذ في `Infrastructure/External/Client/` |
| **Client**      | `IGeoLocationService`      | تحديد الموقع من الـ IP               |
| **Client**      | `IUserAgentParser`         | تحليل الـ User-Agent                  |
| **Email**       | `IEmailSender`             | إرسال البريد الإلكتروني              |
| **FileStorage** | `IFileStorage`             | رفع/حذف ملفات محلياً                 |
| **Jobs**        | `IJobScheduler`            | جدولة Hangfire jobs                  |
| **Notifications** | `ISupportRequestNotifier` | إشعار فريق الدعم باستفسار جديد      |
| **Notifications** | `ISupportAnswerNotifier` | إشعار المستخدم بإجابة الفريق         |
| **Realtime**    | `IChatNotifier`            | إشعارات الشات اللحظي (SignalR)       |
| **Routing**     | `IApplicationUrlService`   | توليد الروابط الأساسية               |
| **Settings**    | `ISiteSettingsService`     | قراءة إعدادات الموقع                  |
| **Storage**     | `ICourseContentStorage`    | تخزين ملفات الكورسات (Google Drive)  |

#### IChatNotifier — واجهة الشات اللحظي

```csharp
public interface IChatNotifier
{
    Task NotifyMessageReceivedAsync(Guid recipientUserId, MessageDto message, CancellationToken ct = default);
    Task NotifyMessagesReadAsync(Guid recipientUserId, MessagesReadDto payload, CancellationToken ct = default);
}
```

#### ICourseContentStorage — واجهة تخزين المحتوى

```csharp
public interface ICourseContentStorage
{
    Task<Result<CourseContentUploadResult>> UploadAsync(Stream fileStream, string fileName, string folderName, CancellationToken ct = default);
    Task<Result<CourseContentDownloadResult>> DownloadAsync(string fileId, CancellationToken ct = default);
    Task<Result> DeleteAsync(string fileId, CancellationToken ct = default);
    Task<Result<DriveQuotaUsage>> GetQuotaUsageAsync(CancellationToken ct = default);
    Task<Result<string>> EnsureSessionFolderAsync(Guid sessionId, CancellationToken ct = default);
}
```

---

## 4. Infrastructure Layer

### 4.1 Persistence (EF Core + SQL Server)

**File**: `src/Skill-Loop.Infrastructure/Persistence/Data/AppDbContext.cs`

```csharp
public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    // Instructor Profiles
    public DbSet<InstructorProfile> InstructorProfiles => Set<InstructorProfile>();
    public DbSet<InstructorReview> InstructorReviews => Set<InstructorReview>();
    public DbSet<InstructorAvailability> InstructorAvailabilities => Set<InstructorAvailability>();

    // Core
    public DbSet<SiteSettings> SiteSettings => Set<SiteSettings>();
    public DbSet<OtpVerification> OtpVerifications => Set<OtpVerification>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<StaffInvitation> StaffInvitations => Set<StaffInvitation>();

    // Courses & Categories
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<CourseBookmark> CourseBookmarks => Set<CourseBookmark>();
    public DbSet<CourseReview> CourseReviews => Set<CourseReview>();

    // Enrollments & Wallets
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<UserWallet> UserWallets => Set<UserWallet>();
    public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();

    // Sessions & Bookings
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<SessionMaterial> SessionMaterials => Set<SessionMaterial>();

    // Chat
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    // Notifications & Support
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<SupportQuestion> SupportQuestions => Set<SupportQuestion>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        builder.ApplySoftDeleteGlobalFilters();
    }
}
```

> **لإضافة كيان جديد**: أضف `DbSet<T>` هنا، وأنشئ Configuration في `Persistence/Configurations/`.

**Configurations**: `src/Skill-Loop.Infrastructure/Persistence/Configurations/`

| Configuration File                      | الكيان/الكيانات                                           |
| --------------------------------------- | --------------------------------------------------------- |
| `ApplicationUserConfiguration.cs`       | ApplicationUser                                           |
| `ApplicationRoleConfiguration.cs`       | ApplicationRole                                           |
| `BookingConfiguration.cs`               | Booking                                                   |
| `CategorySectionLessonConfiguration.cs` | Category, Section, Lesson                                 |
| `ChatConfigurations.cs`                 | Conversation, ChatMessage                                 |
| `CourseConfiguration.cs`                | Course (Aggregate + Value Objects)                        |
| `EnrollmentAndWalletConfigurations.cs`  | Enrollment, LessonProgress, UserWallet, WalletTransaction, CourseBookmark, CourseReview |
| `InstructorProfileConfiguration.cs`     | InstructorProfile (1-to-1 مع ApplicationUser)             |
| `InstructorReviewConfiguration.cs`      | InstructorReview (1-لـ-متعدد مع InstructorProfile)        |
| `InstructorAvailabilityConfiguration.cs`| InstructorAvailability (1-لـ-متعدد مع InstructorProfile)  |
| `NotificationConfiguration.cs`          | Notification                                             |
| `OtpVerificationConfiguration.cs`       | OtpVerification                                           |
| `OutboxMessageConfiguration.cs`         | OutboxMessage                                             |
| `RefreshTokenConfiguration.cs`          | RefreshToken                                              |
| `SessionConfiguration.cs`               | Session                                                   |
| `SessionMaterialConfiguration.cs`       | SessionMaterial                                           |
| `SiteSettingsConfiguration.cs`          | SiteSettings                                              |
| `StaffInvitationConfiguration.cs`       | StaffInvitation                                           |
| `SupportQuestionConfiguration.cs`       | SupportQuestion                                           |
| `TbPermissionConfiguration.cs`          | TbPermission                                              |
| `TbRolePermissionConfiguration.cs`      | TbRolePermission                                          |

---

### 4.2 Interceptors

**Files**: `src/Skill-Loop.Infrastructure/Persistence/Interceptors/`

| Interceptor                       | مهمته                                                                                            |
| --------------------------------- | ------------------------------------------------------------------------------------------------ |
| `AuditableEntityInterceptor`      | يملأ `CreatedAt/CreatedBy` عند الإضافة و `UpdatedAt/UpdatedBy` عند التعديل لأي `AuditableEntity` |
| `SoftDeleteInterceptor`           | يحوّل `EntityState.Deleted` إلى `Modified` ويضع `IsDeleted=true` لأي `SoftDeleteEntity`          |
| `InsertOutboxMessagesInterceptor` | يجمع الـ `DomainEvents` من الكيانات ← يحولها لـ `OutboxMessage` ← يضيفها للـ Context قبل الحفظ   |

**ترتيب التسجيل** في `AddPersistence.cs`:

```csharp
options.UseSqlServer(...)
       .AddInterceptors(
           softDeleteInterceptor,
           auditableInterceptor,
           insertOutboxInterceptor);
```

---

### 4.3 Outbox Pattern & Hangfire

**Flow**:

```
Entity.AddDomainEvent() → SaveChanges → InsertOutboxMessagesInterceptor
    → OutboxMessage in DB → ProcessOutboxMessagesJob (كل 5 ثوانٍ)
    → DomainEventNotification<T> → MediatR.Publish → EventHandler
```

**OutboxMessage** (`src/Skill-Loop.Infrastructure/Persistence/Outbox/OutboxMessage.cs`):

```csharp
public sealed class OutboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; }         // AssemblyQualifiedName
    public string Content { get; set; }       // JSON
    public DateTime OccurredOnUtc { get; set; }
    public DateTime? ProcessedOnUtc { get; set; }
    public string? Error { get; set; }
    public int RetryCount { get; set; }       // max 3
}
```

**ProcessOutboxMessagesJob** — مجدول كل 5 ثوانٍ:

```csharp
// src/Skill-Loop.Api/Extensions/PipelineExtensions.cs
RecurringJob.AddOrUpdate<ProcessOutboxMessagesJob>(
    "process-outbox-messages",
    job => job.ProcessAsync(),
    "*/5 * * * * *"); // كل 5 ثوانٍ (6-field cron)
```

**IJobScheduler** — واجهة Hangfire:

```csharp
// src/Skill-Loop.Infrastructure/External/Jobs/HangfireJobScheduler.cs
public class HangfireJobScheduler : IJobScheduler
{
    public void Enqueue<T>(Expression<Action<T>> methodCall)
        => BackgroundJob.Enqueue(methodCall);
}
```

---

### 4.4 Dependency Injection

**Main file**: `src/Skill-Loop.Infrastructure/DependencyInjection/DependencyInjection.cs`

يستدعي partial methods:

```
AddInfrastructure()
  ├── AddCoreServices()         ← IDateTime, ICurrentUser, ICorrelationContext, IGeoLocationService,
  │                                IIdentityNotificationService, IInvitationService, ISiteSettingsService,
  │                                IApplicationUrlService, IClientContext, IUserAgentParser,
  │                                ISupportRequestNotifier, ISupportAnswerNotifier
  ├── AddCaching()              ← RedisCacheService (⚠️ the MemoryCacheService fallback is commented out — see §8)
  ├── AddPersistence()          ← AppDbContext + Interceptors
  ├── AddHangfireJobs()         ← Hangfire + JobScheduler
  ├── AddIdentityServices()     ← ASP.NET Identity + tokens + permissions + user management
  ├── AddJwtAuthentication()    ← JWT Bearer
  ├── AddExternalAuth()         ← Google Auth
  ├── AddMail()                 ← SMTP
  ├── AddFileStorage()          ← Local File Storage
  ├── AddGoogleDriveStorage()   ← Google Drive Storage (ICourseContentStorage)
  ├── AddBaseUrl()              ← Backend/Frontend URLs
  └── AddOtpService()           ← OTP Settings
```

> `AddDatabaseSeeder()` **مش** جزء من سلسلة `AddInfrastructure()`. هو extension method على `IApplicationBuilder` (`AddDatabaseSeeder.cs:17`) بيتنادى مباشرة من `Program.cs:16` عبر `app.SeedDatabaseAsync()`، وده بيحصل **قبل** بناء الـ HTTP pipeline.

---

## 5. API Layer

### 5.1 BaseApiController

**File**: `src/Skill-Loop.Api/Controllers/Base/BaseApiController.cs`

```csharp
[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    private ISender? _mediator;
    protected ISender Mediator => _mediator ??=
        HttpContext.RequestServices.GetRequiredService<ISender>();

    protected IResult HandleResult(Result result) => result.ToHttpResponse(HttpContext);
    protected IResult HandleResult<T>(Result<T> result) => result.ToHttpResponse(HttpContext);
}
```

> **كل Controller جديد يجب أن يرث من `BaseApiController`** ويستخدم `HandleResult()`.

---

### 5.2 ResultExtensions (ErrorType → HTTP)

**File**: `src/Skill-Loop.Api/Extensions/ResultExtensions.cs`

| ErrorType           | HTTP Status | Title            |
| ------------------- | ----------- | ---------------- |
| `Validation`        | 400         | Validation Error |
| `Unauthorized`      | 401         | Unauthorized     |
| `Forbidden`         | 403         | Forbidden        |
| `NotFound`          | 404         | Not Found        |
| `Conflict`          | 409         | Conflict         |
| `Unexpected`        | 500         | Server Error     |
| `Failure` (default) | 400         | Bad Request      |

**Responses**:

- **Success without data** → `204 NoContent` (أو `200 OK` مع message)
- **Success with data** → `200 OK` مع `{ data: ... }`
- **Failure** → `application/problem+json` مع `ProblemDetails` يشمل `errors[]`, `traceId`, `correlationId`

---

### 5.3 Contracts (Request DTOs)

**Files**: `src/Skill-Loop.Api/Contracts/`

```
Contracts/
├── Common/
│   └── PaginationRequest.cs
├── Auth/
│   └── ...
├── Bookings/
│   └── BookingRequests.cs
├── Categories/
│   └── ...
├── Chat/
│   └── ChatRequests.cs
├── Courses/
│   └── CourseRequests.cs
├── Enrollments/
│   └── EnrollmentRequests.cs
├── Instructors/
│   ├── AddAvailabilityRequest.cs
│   ├── AddInstructorReviewRequest.cs
│   ├── ChangeInstructorApprovalStatusRequest.cs
│   ├── CreateInstructorProfileRequest.cs
│   ├── GetInstructorsRequest.cs
│   ├── UpdateInstructorProfileRequest.cs
│   └── UpdateInstructorReviewRequest.cs
├── Profile/
│   └── ...
├── Sessions/
│   └── ...
├── StaffInvitations/
│   └── ...
├── Support/
│   └── SupportRequests.cs
└── Users/
    └── ...
```

> مفيش مجلد `Skills/` — اللي اسمه كده فيرمي في `Courses/CourseRequests.cs`.

**Convention**:

- اسم الكلاس: `{Action}Request`
- يكون `sealed record` (positional أو init-only)
- يُستخدم فقط في الـ Controller، ولا يخترق طبقة Application — **استثناء واحد**: `SiteSettingsController.UpdateSettings` بيربط `UpdateSiteSettingsCommand` مباشرة كـ `[FromBody]`

---

### 5.4 Controller Pattern

**Controllers الموجودة حالياً**:

| Controller                       | Route                       | الوصف                                               |
| -------------------------------- | --------------------------- | --------------------------------------------------- |
| `AuthController`                 | `api/v1/auth`               | تسجيل الدخول، التوكن، OTP                           |
| `ProfileController`              | `api/v1/me`                 | إدارة الملف الشخصي للمستخدم الحالي                  |
| `UsersController`                | `api/v1/users`              | إدارة المستخدمين (Admin)                            |
| `PermissionManagementController` | `api/v1/permission-management` | إدارة الصلاحيات والأدوار                            |
| `StaffInvitationsController`     | `api/v1/staff-invitations`  | دعوات الموظفين                                      |
| `CategoriesController`           | `api/v1/categories`         | CRUD التصنيفات                                      |
| `CoursesController`              | `api/v1/courses`            | CRUD الكورسات                                       |
| `EnrollmentsController`          | `api/v1/enrollments`        | التسجيل في الكورسات + تقدم الدروس                   |
| `InstructorProfilesController`   | `api/instructor-profiles`   | بروفايل المدرب + التقييمات + اعتماد الإدارة          |
| `WalletsController`              | `api/wallets`               | رصيد المحفظة + سجل الحركات المالية                   |
| `SessionsController`             | `api/v1/sessions`           | CRUD الجلسات التعليمية                              |
| `SessionMaterialsController`     | `api/sessions/{id}/materials` | رفع/حذف/ترتيب ملفات الجلسات                         |
| `BookingsController`             | `api/v1/bookings`           | حجز الجلسات لايف                                     |
| `ChatController`                 | `api/v1/chat`               | المحادثات والرسائل                                  |
| `NotificationsController`        | `api/v1/notifications`      | إشعارات المستخدم + عدد غير المقروء                   |
| `SupportController`              | `api/support`               | الـ FAQ العام + استفسارات المستخدم المسجّل            |
| `SupportManagementController`    | `api/support/manage`        | إدارة الاستفسارات لفريق الدعم (رد/نشر/تعديل/حذف)   |
| `SiteSettingsController`         | `api/SiteSettings`          | إعدادات الموقع — مفيش `[Route]`، فبيترث `api/[controller]` من `BaseApiController` |
| `DevController`                  | `api/v1/dev`                | تطوير فقط — quick-login (محمي بـ `IsDevelopment()`) |
| `TestFilesController`            | `api/test/files`            | endpoints اختبار الـ file storage                    |

**مثال كامل — Controller Pattern**:

```csharp
[Route("api/v1/[controller]")]
public class CoursesController : BaseApiController
{
    [HttpPost]
    [Authorize]
    public async Task<IResult> Create(
        [FromBody] CreateCourseRequest request,
        CancellationToken cancellationToken)
    {
        // 1. Map Contract → Command
        var command = new CreateCourseCommand(...);

        // 2. Send via MediatR
        var result = await Mediator.Send(command, cancellationToken);

        // 3. Convert Result → HTTP Response
        return HandleResult(result);
    }
}
```

**القواعد**:

- الـ Controller **لا يحتوي على منطق أعمال** — فقط mapping ثم `Mediator.Send()` ثم `HandleResult()`
- Route: `[Route("api/v1/[controller]")]` على الكلاس (أو على `BaseApiController`) — **ملاحظة**: مش كل الكنترولرات متسقة. اللي **مش** versioned: `InstructorProfilesController` (`api/instructor-profiles`)، `WalletsController` (`api/wallets`)، `SessionMaterialsController` (`api/sessions`)، `SupportController` / `SupportManagementController` (`api/support`)، `TestFilesController` (`api/test/files`)، و `SiteSettingsController` (مفيش route خالص — ورث `api/SiteSettings`)
- كل action ترجع `Task<IResult>` (Minimal API result type)

---

### 5.5 Global Exception Handler

**File**: `src/Skill-Loop.Api/Middlewares/GlobalExceptionHandler.cs`

يُعالج:

1. `DbUpdateConcurrencyException` → 409 Conflict
2. أي استثناء آخر → 500 Internal Server Error

يُرجع `application/problem+json` مع `traceId` و `correlationId`.

---

### 5.6 Pipeline & Routing

**File**: `src/Skill-Loop.Api/Extensions/PipelineExtensions.cs`

```
Pipeline Order:
1. CorrelationIdMiddleware
2. ExceptionHandler
3. UseSerilogLogging()  (extension مخصص في PipelineExtensions، مش UseSerilogRequestLogging)
4. HTTPS Redirection
5. CORS ("AllowFrontend")
6. Static Files (/uploads)
7. OpenAPI/Scalar Documentation
8. Authentication
9. Authorization
10. Hangfire Dashboard (/hangfire)
11. ProcessOutboxMessagesJob (recurring)
12. MapControllers
13. MapChatHub (/hubs/chat)  ← SignalR
```

---

### 5.7 SignalR (Real-time Chat)

الشات اللحظي يعمل عبر SignalR Hub:

- **Hub Path**: `/hubs/chat`
- **Interface**: `IChatNotifier` (في Application layer)
- **التسجيل**: `app.MapChatHub()` في `PipelineExtensions.cs`
- **الأحداث**:
  - `NotifyMessageReceivedAsync` — إشعار بالرسالة الجديدة للمستقبِل
  - `NotifyMessagesReadAsync` — إشعار بقراءة الرسائل

---

## 6. Unit Tests

**File**: `tests/Skill-Loop.UnitTests/`

**الهيكل**:

```
UnitTests/
├── Common/
│   ├── InMemoryDbContextHelper.cs       ← Helper لـ InMemory DbContext (و InMemoryAppDbContext)
│   └── MockCourseContentStorage.cs      ← Mock للـ Google Drive
├── BackgroundJobs/
│   └── RefreshDriveQuotaJobTests.cs
├── External/
│   ├── Cache/
│   │   └── CacheServiceTests.cs
│   ├── Email/
│   │   ├── EmailTemplateEngineTests.cs
│   │   └── EmailSenderTests.cs
│   ├── FileStorage/
│   │   └── LocalFileStorageTests.cs
│   ├── Notifications/
│   │   ├── NotificationServiceTests.cs
│   │   └── SupportNotificationContractsTests.cs   ← ثوابت ISP/DIP (reflection)
│   └── Security/
│       └── OtpServiceTests.cs
└── Features/
    ├── Accounts/                        ← AccountManagement, Authentication, PermissionManagement,
    │                                       StaffAuth, StaffInvitations
    ├── Bookings/                        ← BookingTests + Commands (Create/Cancel/Change/Complete) + Queries
    ├── Categories/                      ← CategoryTests + Commands + Queries
    ├── Chat/                            ← Commands (StartConversation, SendMessage, MarkConversationRead)
    │                                       + Queries (GetMyConversations, GetConversationMessages)
    ├── Courses/
    │   └── CourseAggregateTests.cs
    ├── Enrollments/
    │   └── EnrollmentAndWalletTests.cs
    ├── Instructors/                     ← InstructorProfileTests, InstructorReviewTests,
    │                                       Commands (Profile/Review/Approval/Availability),
    │                                       Queries (Paged/ByUserId/FullProfile), EventHandlers
    ├── Sessions/                        ← SessionTests, SessionMaterialTests, Commands, Queries, Materials
    ├── SiteSettings/                    ← UpdateSiteSettings (handler + validator) + GetSiteSettings
    ├── Support/                         ← SupportQuestionTests, SupportCacheInvalidationTests,
    │                                       Commands (Answer, SendFaqAnswerEmail, SetPublication,
    │                                       SubmitContactForm) + Queries (GetMySupportQuestions,
    │                                       GetPublishedSupportQuestionsPaged)
    └── Wallets/
        └── EventHandlers/SessionCompleted/
            └── CreditInstructorWalletOnSessionCompletedEventHandlerTests.cs
```

> مفيش `Features/Notifications/` — الإشعارات مغطّاة بشكل غير مباشر عبر `ChatMessageSentEventHandler` (ومفيش اختبار مباشر لها).

**Note**: `BookingTests.cs` (entity-level tests) is also present at the root level of `Features/Bookings/`.

**Convention**:

- اسم الكلاس: `{HandlerName}Tests`
- مسار مطابق للـ Handler في Application
- يستخدم `InMemoryDbContextHelper` لإنشاء DbContext
- يستخدم `MockCourseContentStorage` لمحاكاة Google Drive

---

## 7. End-to-End Checklist — Building a New Feature

### 7.1 New Command

**مثال مرجعي**: ChangePassword, CreateCourse

| #   | الملف                 | المسار                                                                         | ملاحظات                                                 |
| --- | --------------------- | ------------------------------------------------------------------------------ | ------------------------------------------------------- |
| 1   | **Command**           | `Application/Features/{Feature}/Commands/{Action}/{Action}Command.cs`          | `sealed record ... : ICommand` أو `ICommand<TResponse>` |
| 2   | **Handler**           | `Application/Features/{Feature}/Commands/{Action}/{Action}CommandHandler.cs`   | `class ... : ICommandHandler<TCommand>`                 |
| 3   | **Validator**         | `Application/Features/{Feature}/Commands/{Action}/{Action}CommandValidator.cs` | `class ... : AbstractValidator<TCommand>`               |
| 4   | **Contract**          | `Api/Contracts/{Feature}/{Action}Request.cs`                                   | `sealed record`                                         |
| 5   | **Controller Action** | `Api/Controllers/{Feature}Controller.cs`                                       | Map Contract → Command → HandleResult                   |
| 6   | **Errors** (اختياري)  | `Application/Common/Errors/{Feature}/{Feature}Errors.cs`                       | `static class`                                          |
| 7   | **Test**              | `tests/.../Commands/{Action}/{Action}CommandHandlerTests.cs`                   |                                                         |

**مثال Command code**:

```csharp
// 1. Command
public sealed record ChangePasswordCommand(
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword) : ICommand;

// 2. Handler
public sealed class ChangePasswordCommandHandler(
    IPasswordService passwordService,
    ICurrentUser currentUser,
    IJobScheduler jobScheduler,
    IClientContext clientContext) : ICommandHandler<ChangePasswordCommand>
{
    public async Task<Result> Handle(
        ChangePasswordCommand request,
        CancellationToken cancellationToken)
    {
        // business logic...
        return Result.Success("تم تغيير كلمة المرور بنجاح");
    }
}

// 3. Validator
public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).ApplyStandardPasswordRules();
        RuleFor(x => x.ConfirmNewPassword).Equal(x => x.NewPassword);
    }
}
```

---

### 7.2 New Query

**مثال مرجعي**: GetAllUsers (مع Pagination + Caching), GetCoursesPaged

| #   | الملف                   | ملاحظات                                                                 |
| --- | ----------------------- | ----------------------------------------------------------------------- |
| 1   | **Query**               | `sealed record ... : IQuery<TResponse>` أو `ICacheableQuery<TResponse>` |
| 2   | **Handler**             | `class ... : IQueryHandler<TQuery, TResponse>`                          |
| 3   | **Validator** (اختياري) |                                                                         |
| 4   | **Response DTO**        | في `DTOs/` أو `Shared/`                                                 |
| 5   | **Contract**            | `Api/Contracts/{Feature}/Get{Feature}sRequest.cs`                       |
| 6   | **Controller Action**   | `[HttpGet]`                                                             |

**مثال Query مع caching**:

```csharp
// Query
public sealed record GetAllUsersQuery(
    int PageNumber, int PageSize, string? Role, string? SearchTerm
) : ICacheableQuery<PagedResult<UserDto>>
{
    public string CacheKey =>
        $"users-list-{PageNumber}-{PageSize}-{Role ?? "all"}-{SearchTerm ?? "none"}";
    public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(2);
    public TimeSpan? AbsoluteExpiration => TimeSpan.FromMinutes(10);
}
```

---

### 7.3 New Entity

| #   | الخطوة                                | المسار                                                                                        |
| --- | ------------------------------------- | --------------------------------------------------------------------------------------------- |
| 1   | أنشئ مجلد الكيان                      | `Domain/Entities/{EntityName}/`                                                               |
| 2   | اختر Base class مناسب                 | `BaseEntity`, `AuditableEntity`, أو `SoftDeleteEntity`                                        |
| 3   | أنشئ Factory Method (إذا يرفع events) | داخل الكيان — `Create(...)` يرجع `Result<T>`                                                  |
| 4   | أضف Enums (إذا لزم)                   | `Domain/Enums/{EnumName}.cs`                                                                  |
| 5   | أضف Value Objects (إذا لزم)           | `Domain/Entities/{EntityName}/ValueObjects/`                                                  |
| 6   | أضف `DbSet<T>`                        | في `AppDbContext.cs`                                                                          |
| 7   | أنشئ Configuration                    | `Infrastructure/Persistence/Configurations/{Entity}Configuration.cs`                          |
| 8   | أنشئ Migration                        | `dotnet ef migrations add Add{Entity} -p src/Skill-Loop.Infrastructure -s src/Skill-Loop.Api` |

**مثال — Course (Aggregate Root مع Rich Domain Model)**:

```csharp
public sealed class Course : SoftDeleteEntity
{
    public string Title { get; private set; } = string.Empty;
    public CoursePrice Price { get; private set; } = CoursePrice.Free();
    public CourseStatus Status { get; private set; } = CourseStatus.Draft;
    public CourseRating Rating { get; private set; } = CourseRating.Empty();
    // ... navigation collections with backing fields ...

    private Course() { }

    public static Result<Course> Create(string title, ...) { ... }
    public Result UpdateDetails(string title, ...) { ... }
    public Result AddSection(string title, int orderIndex) { ... }
    public void Publish() { ... }
}
```

---

### 7.4 New Domain Event + Handler

| #   | الخطوة                       | المسار                                                                                |
| --- | ---------------------------- | ------------------------------------------------------------------------------------- |
| 1   | أنشئ Event record            | `Domain/Entities/{Entity}/Events/{EventName}Event.cs`                                 |
| 2   | ارفع الحدث في Factory Method | `entity.AddDomainEvent(new XyzEvent(...))`                                            |
| 3   | أنشئ EventHandler            | `Application/Features/{Feature}/EventHandlers/{EventName}/{EventName}EventHandler.cs` |

```csharp
// 1. Event
public sealed record CourseEnrolledDomainEvent(
    Guid EnrollmentId, Guid UserId, Guid CourseId, int CreditsPaid) : IDomainEvent;

// 2. في الكيان
public static Result<Enrollment> Create(Guid userId, Guid courseId, int creditsPaid, int totalCourseLessons)
{
    var enrollment = new Enrollment { ... };
    enrollment.AddDomainEvent(new CourseEnrolledDomainEvent(enrollment.Id, userId, courseId, creditsPaid));
    return Result<Enrollment>.Success(enrollment);
}

// 3. Handler
public sealed class CourseEnrolledEventHandler(IJobScheduler jobScheduler)
    : INotificationHandler<DomainEventNotification<CourseEnrolledDomainEvent>>
{
    public Task Handle(DomainEventNotification<CourseEnrolledDomainEvent> notification, CancellationToken ct)
    {
        // Send notification, update stats, etc.
        return Task.CompletedTask;
    }
}
```

---

### 7.5 New External Service

| #   | الخطوة              | المسار                                                                            |
| --- | ------------------- | --------------------------------------------------------------------------------- |
| 1   | أنشئ Interface      | `Application/Common/Abstractions/External/{Category}/I{Service}.cs`               |
| 2   | أنشئ Implementation | `Infrastructure/External/{Category}/{Service}.cs`                                 |
| 3   | سجّل في DI          | `Infrastructure/DependencyInjection/Add{Category}.cs` أو `DependencyInjection.cs` |

**Categories الموجودة**: `Cache`, `Client`, `Email`, `FileStorage`, `Jobs`, `Realtime`, `Routing`, `Storage`

---

### 7.6 New Hangfire Job

| #   | الخطوة            | المسار                                                            |
| --- | ----------------- | ----------------------------------------------------------------- |
| 1   | أنشئ Job class    | `Infrastructure/BackgroundJobs/{JobName}Job.cs`                   |
| 2   | سجّل كـ Scoped    | في `AddPersistence.cs` أو `AddHangfireJobs.cs`                    |
| 3   | جدوله (اختياري)   | في `PipelineExtensions.cs` عبر `RecurringJob.AddOrUpdate<T>(...)` |
| 4   | أو استخدمه مباشرة | عبر `IJobScheduler.Enqueue<T>(...)`                               |

---

### 7.7 New Permission

| #   | الخطوة                                 | المسار                            |
| --- | -------------------------------------- | --------------------------------- |
| 1   | أضف nested class في Permissions        | `Domain/Constants/Permissions.cs` |
| 2   | أضف const strings                      |                                   |
| 3   | استخدم `[Permission("Module.Action")]` | على الـ Command/Query             |

```csharp
// في Permissions.cs
public static class Courses
{
    public const string Create = "Courses.Create";
    public const string Update = "Courses.Update";
    public const string Delete = "Courses.Delete";
    public const string Publish = "Courses.Publish";
}
```

> الـ Seed يلتقطها تلقائياً عبر Reflection ويضيفها لجدول `TbPermissions` ويربطها بـ SuperAdmin.

---

## 8. Inconsistencies & Risks

### 🔴 Critical

| #   | المشكلة                                      | الملف/المسار                 | التفاصيل                                                                                      |
| --- | -------------------------------------------- | ---------------------------- | --------------------------------------------------------------------------------------------- |
| 1   | **appsettings.json يحتوي أسرار (secrets)**   | `Api/appsettings.json`       | JWT Key, SMTP Password, Google ClientId — يجب نقلها لـ User Secrets أو Environment Variables. الملف نفسه **مُستثنى** في `.gitignore:488`، فالمشكلة هنا القيم في الـ working tree/local commits، لا تتبّع git. |

### 🟡 Medium

| #   | المشكلة                                                           | الملف/المسار                                                                                                                                                                                |
| --- | ----------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 2   | **TestFilesController — كنترولر اختبار في production**            | `Api/Controllers/Test/TestFilesController.cs` — يتيح رفع/حذف ملفات بدون أي authorization. يجب إزالته أو تقييده ببيئة Development.                                                           |
| 3   | **TransactionBehavior لا يُفعّل على ICommand (بدون TResponse)**   | `TransactionBehavior.cs` — العقد `where TRequest : ICommand<TResponse>` يستبعد `ICommand` (بدون generic). أوامر مثل `ChangePasswordCommand : ICommand` **لن تُلف بـ Transaction تلقائياً**. |
| 4   | **SessionMaterial entity uses public setters**                    | `Domain/Entities/Session/SessionMaterial.cs` — يستخدم `{ get; set; }` بدلاً من `{ get; private set; }` — يخالف نمط باقي الكيانات. `Session.cs` نفسه يستخدم `private set` بشكل صحيح.                 |
| 5   | **Redis caching fallback bug**                                    | `Infrastructure/DependencyInjection/AddCaching.cs:40` — إذا فشل الاتصال بـ Redis، `MemoryCacheService` fallback مُعلّق (commented out) → لا يتم تسجيل أي `ICacheService` → فشل DI.                 |
| 6   | **`UsersController` و `StaffInvitationsController` بدون حماية**        | `//[Authorize(...)]` **مُعلّق** في `UsersController.cs:15` و `StaffInvitationsController.cs:21` → endpoints إدارة المستخدمين وإرسال الدعوات متاحة لـ anonymous لحد ما الحماية ترجع. |
| 7   | **`RefreshDriveQuotaJob` مش مجدول**                              | `Infrastructure/BackgroundJobs/RefreshDriveQuotaJob.cs` مسجّل في DI بس مفيش `RecurringJob` بيشغّله (الـ recurring الوحيد هو `ProcessOutboxMessagesJob`) → مراقبة حصة Google Drive مش شغالة أصلاً. |
| 8   | **`CompleteBookingCommand` مالهوش endpoint**                     | `BookingsController` بيعرض `PATCH /{id}/status` بس — الـ Command والـ Handler والـ Validator موجودين ومختبرين بس غير معرّفين في الـ API. |

### 🟢 Minor / Convention

| #   | المشكلة                                                                                                                        |
| --- | ------------------------------------------------------------------------------------------------------------------------------ |
| 9   | **`RefreshDriveQuotaJobConstants.CronExpression` مش مستخدم** — الـ constant معرّف بس مفيش أي كود بيستهلكه.                        |
| 10  | **بقايا ملفات merge reject** لازم تتحذف: `src/Skill-Loop.Domain/Entities/Chat/ChatMessage.cs.rej` و `tests/Skill-Loop.UnitTests/Common/InMemoryDbContextHelper.cs.rej`. |
| 11  | **مجلد `uploads/` غير مُضاف للـ `.gitignore`** — `Logs/` (سطر 486) و `UploadedFiles/` (سطر 485) و `appsettings.json` (سطر 488) و `*.user` (سطر 12) كلهم متغطّيين فعلاً؛ `uploads/` وحده هو الفاتح. |

---

## 9. Module Gap Analysis (Target vs Current)

بناءً على تحليل SkillLoop Backend المستهدف مقارنة بالكود الموجود:

### ✅ Exists (موجود ومُطبّق)

| Module                         | الحالة   | التفاصيل                                                                                                                                                                              |
| ------------------------------ | -------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Auth — Staff Login**         | ✅ مكتمل | `StaffLoginCommand`, `StaffGoogleLoginCommand`, `RefreshTokenCommand`, `LogoutCommand`                                                                                                |
| **Auth — Password Management** | ✅ مكتمل | `ChangePassword`, `ForgotPassword`, `ResetPassword`                                                                                                                                   |
| **Users — Account Management** | ✅ مكتمل | `GetAllUsers`, `GetMyProfile`, `UpdateMyProfile`, `UpdateMyProfilePicture`, `ActivateUser`, `DeactivateUser`, `AssignRoleToUser`, `RemoveRoleFromUser`                                |
| **Auth — Staff Invitations**   | ✅ مكتمل | `SendInvitation`, `AcceptInvitation`, `AcceptInvitationWithGoogle`, `ValidateInvitation`, `StaffInvitationCreatedEventHandler`                                                        |
| **Permission Management**      | ✅ مكتمل | `AssignPermissionToRole`, `RemovePermissionFromRole`, `UpdateRolePermissions`, `GetAllPermissions`, `GetRolePermissions`, `GetAllRolesWithPermissions`                                |
| **OTP Verification**           | ✅ مكتمل | كيان `OtpVerification` + `OtpService` في Infrastructure                                                                                                                               |
| **Categories**                 | ✅ مكتمل | `CreateCategory`, `UpdateCategory`, `DeleteCategory`, `GetCategories` + Entity + Controller                                                                                           |
| **Courses**                    | ✅ مكتمل | Entity (Aggregate Root) + Value Objects + `CreateCourse`, `AddLesson`, `AddCourseReview`, `PublishCourse`, `ToggleCourseBookmark`, `GetCourseById`, `GetCoursesPaged` + Domain Events |
| **Enrollments**                | ✅ مكتمل | Entity + `EnrollInCourse`, `UpdateLessonProgress`, `GetUserEnrolledCourses` + Domain Events + Wallet Integration                                                                      |
| **Sessions**                   | ✅ مكتمل | Entity + `CreateSession`, `UpdateSession`, `DeleteSession`, `ChangeSessionStatus`, `GetSessionById`, `GetSessionsPaged`, `GetMySessionsPaged` + `SessionLocationType` enum, `SessionCompletedDomainEvent`                              |
| **Session Materials**          | ✅ مكتمل | Entity + `UploadSessionMaterial`, `DeleteSessionMaterial`, `ReorderSessionMaterials`, `GetSessionMaterials`, `GetSessionMaterialDownloadInfo` + Google Drive + Domain Event           |
| **Chat**                       | ✅ مكتمل | Entities (`Conversation`, `ChatMessage`) + `StartConversation`, `SendMessage`, `MarkConversationRead`, `GetMyConversations`, `GetConversationMessages` + SignalR Hub                  |
| **Instructor Profile**         | ✅ مكتمل | Entities (`InstructorProfile`, `InstructorReview`, `InstructorAvailability`) + `CreateMyInstructorProfile`, `UpdateMyInstructorProfile`, `ChangeInstructorApprovalStatus`, `AddInstructorAvailability`, `RemoveInstructorAvailability` + Reviews CRUD + `GetInstructorsPaged`, `GetInstructorProfileByUserId`, `GetInstructorFullProfileByUserId` + EventHandlers (`SessionCompleted`, `CourseEnrolled`) + Controller |
| **Wallet**                     | ⚠️ جزئي   | Entity (`UserWallet`, `WalletTransaction`) + `GetMyWallet`, `GetMyWalletTransactionsPaged` + `CreditInstructorWalletEventHandler` + `CreditInstructorWalletOnSessionCompletedEventHandler` + `WalletsController` + Optimistic Concurrency + Domain Events. **Missing**: `BuyCreditsCommand`, `ApplyPromoCodeCommand`, payment gateway integration. |
| **Booking**                    | ✅ مكتمل | Entity (rich aggregate with domain events + lifecycle) + `CreateBooking`, `CancelBooking`, `CompleteBooking`, `ChangeBookingStatus` + `GetBookingById`, `GetMyBookings`, `GetSessionBookings` + `BookingsController` |
| **Support**                      | ✅ مكتمل | Entity (`SupportQuestion`) + `SubmitContactForm`, `CreateSupportQuestion`, `AnswerSupportQuestion`, `UpdateSupportQuestion`, `SetSupportQuestionPublication`, `DeleteSupportQuestion`, `SendFaqAnswerEmail` + `GetPublishedSupportQuestionsPaged`, `GetSupportQuestionById`, `GetMySupportQuestions`, `GetSupportQuestionsPaged`, `GetSupportQuestionDetails` + `SupportNotificationService` — 3 قوالب إيميل: `ContactFormConfirmation`, `SupportNotification`, `AnswerNotification` — تفاصيل في [Support_Subsystem.md](Support_Subsystem.md) |
| **File Storage**               | ✅ مكتمل | `LocalFileStorage` + `GoogleDriveStorage` (ICourseContentStorage)                                                                                                                     |
| **Email Notifications**        | ✅ مكتمل | `SmtpEmailSender` + HTML Templates + `IdentityNotificationService` + `SupportNotificationService`                                                                                     |
| **SiteSettings**               | ✅ مكتمل | Entity + `UpdateSiteSettings`, `GetSiteSettings` + Controller                                                                                                                         |
| **Infrastructure Core**        | ✅ مكتمل | Caching, Logging, CorrelationId, JWT, Hangfire, Outbox Pattern, GeoLocation, UserAgent Parsing                                                                                        |

### ❌ Missing (غير موجود — مطلوب بناؤه)

| Module                   | الأولوية  | ما يجب بناؤه                                                                                                                                      |
| ------------------------ | --------- | ------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Payments**             | 🟡 متوسطة | تكامل مع بوابة دفع + كيان `Payment` + ربط مع Wallet.                                                                                              |
| **Notifications (Push)** | 🟡 متوسطة | Push notifications للموبايل (Firebase FCM). البنية التحتية للبريد وSignalR موجودة لكن push غير موجود.                                             |
| **Reviews (standalone)** | 🟢 منخفضة | `CourseReview` موجود كجزء من Course aggregate. قد يحتاج endpoints مستقلة للتعديل/الحذف.                                                           |

### 📋 Summary Table

```
Module                  | Domain | Application | Infrastructure | API | Tests
------------------------|--------|-------------|----------------|-----|-----
Auth (Staff)            |   ✅   |     ✅      |       ✅       |  ✅  |  ✅
Auth (Users)            |   ✅   |     ✅      |       ✅       |  ✅  |  ✅
Users/Profile           |   ✅   |     ✅      |       ✅       |  ✅  |  ✅
Instructor Profile      |   ✅   |     ✅      |       ✅       |  ✅  |  ✅
Categories              |   ✅   |     ✅      |       ✅       |  ✅  |  ✅
Courses                 |   ✅   |     ✅      |       ✅       |  ✅  |  ✅
Enrollments             |   ✅   |     ✅      |       ✅       |  ✅  |  ✅
Sessions                |   ✅   |     ✅      |       ✅       |  ✅  |  ✅
Session Materials       |   ✅   |     ✅      |       ✅       |  ✅  |  ✅
Booking                 |   ✅   |     ✅      |       ✅       |  ✅  |  ✅
Wallet                  |   ✅   |     ⚠️      |       ✅       |  ✅  |  ⚠️
Payments                |   ⬜   |     ⬜      |       ⬜       |  ⬜  |  ⬜
Chat                    |   ✅   |     ✅      |       ✅       |  ✅  |  ✅
Support (FAQ + Contact) |   ✅   |     ✅      |       ✅       |  ✅  |  ✅
Notifications (Push)    |   ⬜   |     ⬜      |       ⬜       |  ⬜  |  ⬜
Permission Management   |   ✅   |     ✅      |       ✅       |  ✅  |  ✅
OTP                     |   ✅   |     ✅      |       ✅       |  ✅  |  ✅
SiteSettings            |   ✅   |     ✅      |       ✅       |  ✅  |  ✅
File Storage            |   —    |     —       |       ✅       |  ✅  |  ✅
Google Drive Storage    |   —    |     ✅      |       ✅       |  —   |  ✅
Email             |   —    |     —       |       ✅       |  —   |  ✅

✅ = Implemented   ⚠️ = Partial/Needs Work   ⬜ = Missing
```

---

> **عند طلب بناء feature جديدة**: سأتبع هذا الدليل خطوة بخطوة، مع استخدام الـ patterns الموجودة كمرجع:
>
> - **Commands**: ChangePassword (بسيط) أو CreateCourse (مع Aggregate Root)
> - **Queries**: GetCoursesPaged (مع Pagination) أو GetAllUsers (مع Caching + Pagination)
> - **Domain Events**: CourseEnrolledDomainEvent (مع خصم كريديت) أو SessionMaterialUploadedEvent
> - **Aggregate Root**: Course (Rich Domain Model مع Value Objects)
> - **Real-time**: Chat pattern (SignalR + IChatNotifier)
