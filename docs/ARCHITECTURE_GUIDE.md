# SkillLoop Backend — Architecture Guide

> **Purpose**: دليل شامل لأي مطوّر جديد يشرح كيف يبني Feature كاملة في هذا الـ Codebase.
> كل قاعدة مكتوبة هنا مأخوذة من الكود الفعلي، والمسارات مرفقة.

---

## Table of Contents

1. [Solution Structure](#1-solution-structure)
2. [Domain Layer](#2-domain-layer)
   - 2.1 [Result Pattern](#21-result-pattern)
   - 2.2 [Entity Hierarchy](#22-entity-hierarchy)
   - 2.3 [Domain Events](#23-domain-events)
   - 2.4 [Constants (Roles & Permissions)](#24-constants-roles--permissions)
3. [Application Layer](#3-application-layer)
   - 3.1 [CQRS Messaging Abstractions](#31-cqrs-messaging-abstractions)
   - 3.2 [Pipeline Behaviors (MediatR)](#32-pipeline-behaviors-mediatr)
   - 3.3 [Error Convention](#33-error-convention)
   - 3.4 [Pagination](#34-pagination)
   - 3.5 [Caching Interfaces](#35-caching-interfaces)
   - 3.6 [Feature Folder Structure](#36-feature-folder-structure)
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
│   ├── Skill-Loop.Domain/           ← الطبقة الداخلية: الكيانات، القيم، الأحداث
│   ├── Skill-Loop.Application/      ← منطق الأعمال: Commands, Queries, Behaviors
│   ├── Skill-Loop.Infrastructure/   ← التطبيقات الخارجية: EF Core, Hangfire, Email, Cache
│   └── Skill-Loop.Api/              ← نقطة الدخول: Controllers, Contracts, Middleware
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

| Method | Return Type | Usage |
|--------|-------------|-------|
| `Result.Success(message?)` | `Result` | نجاح بدون بيانات |
| `Result.Failure(Error error)` | `Result` | فشل بخطأ واحد |
| `Result.Failure(IEnumerable<Error>)` | `Result` | فشل بعدة أخطاء |
| `Result.Failure(string)` | `Result` | فشل برسالة (يُحوّل لـ Error مع `ErrorType.Failure`) |
| `Result<T>.Success(T data, message?)` | `Result<T>` | نجاح مع بيانات |
| `Result<T>.Failure(Error error)` | `Result<T>` | فشل بخطأ واحد |

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

| Base Class | متى تستخدمه | Id | Audit | Soft Delete |
|-----------|-------------|-----|-------|-------------|
| `Entity` | كيان بدون Id خاص (نادر) | ❌ | ❌ | ❌ |
| `BaseEntity` | كيان بسيط بـ Guid V7 | ✅ | ❌ | ❌ |
| `AuditableEntity` | كيان يحتاج تتبع الإنشاء والتعديل | ✅ | ✅ | ❌ |
| `SoftDeleteEntity` | كيان يحتاج حذف منطقي (soft delete) | ✅ | ✅ | ✅ |

**ملاحظة**: الـ `Id` يستخدم `Guid.CreateVersion7()` — وهو GUID مرتب زمنياً.

```csharp
// BaseEntity.cs
public abstract class BaseEntity : Entity
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
}
```

**مثال حقيقي**: `StaffInvitation` يرث من `AuditableEntity`:
```csharp
// src/Skill-Loop.Domain/Entities/Invitation/StaffInvitation.cs
public class StaffInvitation : AuditableEntity { ... }
```

`OtpVerification` يرث من `BaseEntity`:
```csharp
// src/Skill-Loop.Domain/Entities/OtpVerification/OtpVerification.cs
public class OtpVerification : BaseEntity { ... }
```

---

### 2.3 Domain Events

**Files**:
- `src/Skill-Loop.Domain/Common/Events/IDomainEvent.cs` — واجهة فارغة (marker)
- `src/Skill-Loop.Domain/Common/Events/IHasDomainEvents.cs` — عقد للكيانات التي ترفع أحداث

**الآلية**:
1. الكيان يرفع حدث عبر `AddDomainEvent()` (متاحة من `Entity`)
2. يتم جمع الأحداث عبر `InsertOutboxMessagesInterceptor` قبل `SaveChanges` ← تُخزّن في جدول `OutboxMessages`
3. `ProcessOutboxMessagesJob` (Hangfire) يقرأ الـ Outbox ← يغلّف الحدث في `DomainEventNotification<T>` ← ينشره عبر `IPublisher.Publish()`

**مثال كامل — StaffInvitation يرفع حدث**:

```csharp
// src/Skill-Loop.Domain/Entities/Invitation/Events/StaffInvitationCreatedEvent.cs
public sealed record StaffInvitationCreatedEvent(StaffInvitation Invitation) : IDomainEvent;

// src/Skill-Loop.Domain/Entities/Invitation/StaffInvitation.cs
public static StaffInvitation Create(...)
{
    var invitation = new StaffInvitation { ... };
    invitation.AddDomainEvent(new StaffInvitationCreatedEvent(invitation));
    return invitation;
}
```

**الـ Handler في Application**:
```csharp
// src/Skill-Loop.Application/Features/Accounts/StaffInvitations/EventHandlers/
//   StaffInvitationCreated/StaffInvitationCreatedEventHandler.cs
public sealed class StaffInvitationCreatedEventHandler(IJobScheduler jobScheduler)
    : INotificationHandler<DomainEventNotification<StaffInvitationCreatedEvent>>
{
    public Task Handle(DomainEventNotification<StaffInvitationCreatedEvent> notification, ...)
    {
        var invitation = notification.DomainEvent.Invitation;
        // ... يجدول إرسال بريد عبر Hangfire
    }
}
```

---

### 2.4 Constants (Roles & Permissions)

**Files**:
- `src/Skill-Loop.Domain/Constants/Roles.cs`
- `src/Skill-Loop.Domain/Constants/Permissions.cs`

```csharp
// Roles.cs — الأدوار المتاحة
public static class Roles
{
    public const string Admin = "Admin";
    public static readonly IReadOnlyList<string> All = new[] { Admin };
}
```

```csharp
// Permissions.cs — صلاحيات حسب الوحدات (Nested Classes)
public class Permissions
{
    // الآن فارغة — لا يوجد nested classes بعد!
    // لكن الدالة السحرية تجلب كل الصلاحيات عبر Reflection:
    public static IReadOnlyList<string> GetAllPermissions() { ... }
}
```

> **طريقة إضافة صلاحية جديدة**: أضف `nested static class` بداخل `Permissions` وأضف `const string` بداخلها. الـ Seed يلتقطها تلقائياً عبر Reflection.

**Seeding** (`src/Skill-Loop.Infrastructure/Persistence/Seed/ContextSeed.cs`):
1. ينشئ الأدوار من `Roles.All`
2. يجلب كل الصلاحيات من `Permissions.GetAllPermissions()` ويضيف الجديد لجدول `TbPermission`
3. يربط كل الصلاحيات بدور `Admin` عبر جدول `TbRolePermission`

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

| # | Behavior | مهمته | يُفعّل على |
|---|----------|-------|-----------|
| 1 | `LoggingBehavior` | يسجل بداية/نهاية كل Request مع CorrelationId | كل Request |
| 2 | `PerformanceBehavior` | يحذر إذا تجاوز الطلب 800ms | كل Request |
| 3 | `AuthorizationBehavior` | يفحص `[Permission]` attribute ← يتحقق من `ICurrentUser` | فقط إذا وُجد `[PermissionAttribute]` |
| 4 | `ValidationBehavior` | يجمع أخطاء FluentValidation ← يرجع `Result.Failure` | إذا وُجد `IValidator<TRequest>` |
| 5 | `CachingBehavior` | يقرأ من الكاش (GetOrCreateAsync) | فقط `ICacheableQuery` |
| 6 | `CacheInvalidationBehavior` | يمسح الكاش بالـ prefix بعد النجاح | فقط `ICacheInvalidatorCommand` |
| 7 | `TransactionBehavior` | يلف الـ Handler في DB transaction | فقط `ICommand<TResponse> where TResponse : Result` |

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
[Permission("Skills.Create")]
public sealed record CreateSkillCommand(...) : ICommand<Guid>;
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
├── Files/
│   └── FileErrors.cs
├── Identity/
│   ├── ExternalAuthErrors.cs
│   ├── OtpErrors.cs
│   ├── PasswordErrors.cs
│   ├── TokenErrors.cs
│   └── UserErrors.cs
└── Invitations/
    └── InvitationErrors.cs
```

**Convention**:
- اسم الكلاس: `{Domain}Errors` (مثل `UserErrors`, `FileErrors`)
- الكلاس `static class`
- الأخطاء الثابتة: `public static readonly Error`
- الأخطاء الديناميكية: `public static Error MethodName(string details) => new(...)`
- كود الخطأ: `"DOMAIN_ERROR_CODE"` بحروف كبيرة وفواصل underscores
- الوصف: نص عربي واضح

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

**مثال حقيقي — GetAllUsersQuery**:
```csharp
// الـ Query يرث PaginationRequest (مباشرة) أو يأخذ الخصائص:
public sealed record GetAllUsersQuery(
    int PageNumber, int PageSize, string? Role, string? SearchTerm
) : ICacheableQuery<PagedResult<UserDto>> { ... }

// الـ Handler:
public async Task<Result<PagedResult<UserDto>>> Handle(...)
{
    var pagedResult = await _userService.GetAllUsersAsync(...);
    return Result<PagedResult<UserDto>>.Success(pagedResult);
}
```

**الـ Contract في Api**:
```csharp
// src/Skill-Loop.Api/Contracts/Accounts/GetUsersRequest.cs
public record GetUsersRequest : PaginationRequest
{
    public string? Role { get; init; }
    public string? SearchTerm { get; init; }
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
    └── Shared/
        └── {Feature}Response.cs  (DTOs مشتركة)
```

---

## 4. Infrastructure Layer

### 4.1 Persistence (EF Core + SQL Server)

**File**: `src/Skill-Loop.Infrastructure/Persistence/Data/AppDbContext.cs`

```csharp
public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public DbSet<OtpVerification> OtpVerifications => Set<OtpVerification>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<StaffInvitation> StaffInvitations => Set<StaffInvitation>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        builder.ApplySoftDeleteGlobalFilters(); // فلتر عام للحذف المنطقي
    }
}
```

> **لإضافة كيان جديد**: أضف `DbSet<T>` هنا، وأنشئ Configuration في `Persistence/Configurations/`.

**Configurations**: `src/Skill-Loop.Infrastructure/Persistence/Configurations/`
- كل Configuration تطبق `IEntityTypeConfiguration<T>`
- مثال: `StaffInvitationConfiguration.cs` يحدد اسم الجدول، الأطوال، الفهارس

```csharp
public sealed class StaffInvitationConfiguration : IEntityTypeConfiguration<StaffInvitation>
{
    public void Configure(EntityTypeBuilder<StaffInvitation> builder)
    {
        builder.ToTable("StaffInvitations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(256);
        // ...
        builder.HasIndex(x => x.Token).IsUnique();
    }
}
```

---

### 4.2 Interceptors

**Files**: `src/Skill-Loop.Infrastructure/Persistence/Interceptors/`

| Interceptor | مهمته |
|-------------|-------|
| `AuditableEntityInterceptor` | يملأ `CreatedAt/CreatedBy` عند الإضافة و `UpdatedAt/UpdatedBy` عند التعديل لأي `AuditableEntity` |
| `SoftDeleteInterceptor` | يحوّل `EntityState.Deleted` إلى `Modified` ويضع `IsDeleted=true` لأي `SoftDeleteEntity` |
| `InsertOutboxMessagesInterceptor` | يجمع الـ `DomainEvents` من الكيانات ← يحولها لـ `OutboxMessage` ← يضيفها للـ Context قبل الحفظ |

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
    // ... overloads for Task
}
```

---

### 4.4 Dependency Injection

**Main file**: `src/Skill-Loop.Infrastructure/DependencyInjection/DependencyInjection.cs`

يستدعي partial methods:
```
AddInfrastructure()
  ├── AddCoreServices()         ← IDateTime, ICurrentUser, ICorrelationContext, etc.
  ├── AddCaching()              ← MemoryCacheService
  ├── AddPersistence()          ← AppDbContext + Interceptors
  ├── AddHangfireJobs()         ← Hangfire + JobScheduler
  ├── AddIdentityServices()     ← ASP.NET Identity
  ├── AddJwtAuthentication()    ← JWT Bearer
  ├── AddExternalAuth()         ← Google Auth
  ├── AddMail()                 ← SMTP
  ├── AddFileStorage()          ← Local File Storage
  ├── AddBaseUrl()              ← Backend/Frontend URLs
  └── AddOtpService()           ← OTP Settings
```

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

| ErrorType | HTTP Status | Title |
|-----------|-------------|-------|
| `Validation` | 400 | Validation Error |
| `Unauthorized` | 401 | Unauthorized |
| `Forbidden` | 403 | Forbidden |
| `NotFound` | 404 | Not Found |
| `Conflict` | 409 | Conflict |
| `Unexpected` | 500 | Server Error |
| `Failure` (default) | 400 | Bad Request |

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
│   └── PaginationRequest.cs      ← base record للـ paginated requests
├── Accounts/
│   ├── ChangePasswordRequest.cs
│   ├── GetUsersRequest.cs        ← يرث PaginationRequest
│   └── ...
├── Skills/
│   ├── CreateSkillRequest.cs     ← (فارغ حالياً)
│   └── ...
└── StaffInvitations/
    └── ...
```

**Convention**:
- اسم الكلاس: `{Action}Request`
- يكون `sealed record` (positional أو init-only)
- يُستخدم فقط في الـ Controller، ولا يخترق طبقة Application

---

### 5.4 Controller Pattern

**مثال كامل — من AccountsController**:

```csharp
// src/Skill-Loop.Api/Controllers/AccountsController.cs
[Route("api/[controller]")]
public class AccountsController : BaseApiController
{
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        // 1. Map Contract → Command
        var command = new ChangePasswordCommand(
            request.CurrentPassword,
            request.NewPassword,
            request.ConfirmNewPassword);

        // 2. Send via MediatR
        var result = await Mediator.Send(command, cancellationToken);

        // 3. Convert Result → HTTP Response
        return HandleResult(result);
    }
}
```

**القواعد**:
- الـ Controller **لا يحتوي على منطق أعمال** — فقط mapping ثم `Mediator.Send()` ثم `HandleResult()`
- Route: `[Route("api/[controller]")]` على الكلاس (أو على `BaseApiController`)
- كل action ترجع `Task<IResult>` (Minimal API result type)

> **ملاحظة**: لا يوجد API Versioning مُفعّل حالياً. الـ routes تستخدم `api/[controller]` فقط.

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
3. SerilogRequestLogging
4. HTTPS Redirection
5. CORS ("AllowFrontend")
6. Static Files (/uploads)
7. OpenAPI/Scalar Documentation
8. Authentication
9. Authorization
10. Hangfire Dashboard (/hangfire)
11. ProcessOutboxMessagesJob (recurring)
12. MapControllers
```

---

## 6. Unit Tests

**File**: `tests/Skill-Loop.UnitTests/`

**الهيكل يطابق Application Layer**:
```
UnitTests/
├── Common/
│   └── MockDbContextHelper.cs   ← (فارغ حالياً — stub)
└── Features/
    ├── Accounts/
    │   ├── AccountManagement/
    │   │   ├── Commands/
    │   │   │   ├── ChangePassword/ChangePasswordCommandHandlerTests.cs  ← (stub)
    │   │   │   └── ...
    │   │   └── Queries/
    │   │       └── GetAllUsers/GetAllUsersQueryHandlerTests.cs  ← (stub)
    │   └── ...
    └── Skills/
        ├── Commands/CreateSkill/CreateSkillCommandHandlerTests.cs  ← (stub)
        └── Queries/GetSkills/GetSkillsQueryHandlerTests.cs  ← (stub)
```

> **⚠️ NOTICE**: جميع ملفات الاختبارات حالياً **stubs فارغة** (كلاسات فارغة بدون أي test methods). وكذلك `MockDbContextHelper` فارغ. يجب تعبئتها لاحقاً.

**Convention المتوقع** (بناءً على هيكل الملفات):
- اسم الكلاس: `{HandlerName}Tests`
- مسار مطابق للـ Handler في Application
- يستخدم `MockDbContextHelper` لإنشاء InMemory DbContext

---

## 7. End-to-End Checklist — Building a New Feature

### 7.1 New Command

**مثال مرجعي**: ChangePassword

| # | الملف | المسار | ملاحظات |
|---|-------|--------|---------|
| 1 | **Command** | `Application/Features/{Feature}/Commands/{Action}/{Action}Command.cs` | `sealed record ... : ICommand` أو `ICommand<TResponse>` |
| 2 | **Handler** | `Application/Features/{Feature}/Commands/{Action}/{Action}CommandHandler.cs` | `class ... : ICommandHandler<TCommand>` |
| 3 | **Validator** | `Application/Features/{Feature}/Commands/{Action}/{Action}CommandValidator.cs` | `class ... : AbstractValidator<TCommand>` |
| 4 | **Contract** | `Api/Contracts/{Feature}/{Action}Request.cs` | `sealed record` |
| 5 | **Controller Action** | `Api/Controllers/{Feature}Controller.cs` | Map Contract → Command → HandleResult |
| 6 | **Errors** (اختياري) | `Application/Common/Errors/{Feature}/{Feature}Errors.cs` | `static class` |
| 7 | **Test** | `tests/.../Commands/{Action}/{Action}CommandHandlerTests.cs` | |

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

**مثال مرجعي**: GetAllUsers (مع Pagination + Caching)

| # | الملف | ملاحظات |
|---|-------|---------|
| 1 | **Query** | `sealed record ... : IQuery<TResponse>` أو `ICacheableQuery<TResponse>` |
| 2 | **Handler** | `class ... : IQueryHandler<TQuery, TResponse>` |
| 3 | **Validator** (اختياري) | |
| 4 | **Response DTO** | في `Shared/` أو مكان مناسب |
| 5 | **Contract** | `Api/Contracts/{Feature}/Get{Feature}sRequest.cs` |
| 6 | **Controller Action** | `[HttpGet]` |

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

// Handler
public sealed class GetAllUsersQueryHandler
    : IQueryHandler<GetAllUsersQuery, PagedResult<UserDto>>
{
    public async Task<Result<PagedResult<UserDto>>> Handle(...)
    {
        var pagedResult = await _userService.GetAllUsersAsync(...);
        return Result<PagedResult<UserDto>>.Success(pagedResult);
    }
}
```

---

### 7.3 New Entity

| # | الخطوة | المسار |
|---|--------|--------|
| 1 | أنشئ مجلد الكيان | `Domain/Entities/{EntityName}/` |
| 2 | اختر Base class مناسب | `BaseEntity`, `AuditableEntity`, أو `SoftDeleteEntity` |
| 3 | أنشئ Factory Method (إذا يرفع events) | داخل الكيان |
| 4 | أضف `DbSet<T>` | في `AppDbContext.cs` |
| 5 | أنشئ Configuration | `Infrastructure/Persistence/Configurations/{Entity}Configuration.cs` |
| 6 | أنشئ Migration | `dotnet ef migrations add Add{Entity} -p src/Skill-Loop.Infrastructure -s src/Skill-Loop.Api` |

**مثال Configuration**:
```csharp
public sealed class SkillConfiguration : IEntityTypeConfiguration<Skill>
{
    public void Configure(EntityTypeBuilder<Skill> builder)
    {
        builder.ToTable("Skills");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        // indexes, relationships...
    }
}
```

---

### 7.4 New Domain Event + Handler

| # | الخطوة | المسار |
|---|--------|--------|
| 1 | أنشئ Event record | `Domain/Entities/{Entity}/Events/{EventName}Event.cs` |
| 2 | ارفع الحدث في Factory Method | `entity.AddDomainEvent(new XyzEvent(...))` |
| 3 | أنشئ EventHandler | `Application/Features/{Feature}/EventHandlers/{EventName}/{EventName}EventHandler.cs` |

```csharp
// 1. Event
public sealed record SkillCreatedEvent(Skill Skill) : IDomainEvent;

// 2. في الكيان
public static Skill Create(string name)
{
    var skill = new Skill { Name = name };
    skill.AddDomainEvent(new SkillCreatedEvent(skill));
    return skill;
}

// 3. Handler
public sealed class SkillCreatedEventHandler(IJobScheduler jobScheduler)
    : INotificationHandler<DomainEventNotification<SkillCreatedEvent>>
{
    public Task Handle(
        DomainEventNotification<SkillCreatedEvent> notification,
        CancellationToken cancellationToken)
    {
        // Send notification, update cache, etc.
        return Task.CompletedTask;
    }
}
```

---

### 7.5 New External Service

| # | الخطوة | المسار |
|---|--------|--------|
| 1 | أنشئ Interface | `Application/Common/Abstractions/External/{Category}/I{Service}.cs` |
| 2 | أنشئ Implementation | `Infrastructure/External/{Category}/{Service}.cs` |
| 3 | سجّل في DI | `Infrastructure/DependencyInjection/Add{Category}.cs` أو `DependencyInjection.cs` |

**Categories الموجودة**: `Cache`, `Client`, `Email`, `FileStorage`, `Jobs`, `Routing`

---

### 7.6 New Hangfire Job

| # | الخطوة | المسار |
|---|--------|--------|
| 1 | أنشئ Job class | `Infrastructure/BackgroundJobs/{JobName}Job.cs` |
| 2 | سجّل كـ Scoped | في `AddPersistence.cs` أو `AddHangfireJobs.cs` |
| 3 | جدوله (اختياري) | في `PipelineExtensions.cs` عبر `RecurringJob.AddOrUpdate<T>(...)` |
| 4 | أو استخدمه مباشرة | عبر `IJobScheduler.Enqueue<T>(...)` |

---

### 7.7 New Permission

| # | الخطوة | المسار |
|---|--------|--------|
| 1 | أضف nested class في Permissions | `Domain/Constants/Permissions.cs` |
| 2 | أضف const strings | |
| 3 | استخدم `[Permission("Module.Action")]` | على الـ Command/Query |

```csharp
// في Permissions.cs
public static class Skills
{
    public const string Create = "Skills.Create";
    public const string Update = "Skills.Update";
    public const string Delete = "Skills.Delete";
    public const string View = "Skills.View";
}
```

> الـ Seed يلتقطها تلقائياً عبر Reflection ويضيفها لجدول `TbPermission` ويربطها بـ Admin.

---

## 8. Inconsistencies & Risks

### 🔴 Critical

| # | المشكلة | الملف/المسار | التفاصيل |
|---|---------|-------------|---------|
| 1 | **Skills feature — كل الملفات stubs فارغة** | `Application/Features/Skills/**` | كل الـ Commands, Queries, Handlers, Validators, Response — كلها كلاسات فارغة بدون أي كود. كذلك `Api/Controllers/SkillsController.cs` و `Api/Contracts/Skills/*` فارغة. |
| 2 | **لا يوجد Skill Entity في Domain** | `Domain/Entities/` | يوجد مجلد `Invitation` و `OtpVerification` فقط. لا يوجد كيان `Skill` رغم وجود Feature folder كامل في Application. |
| 3 | **Migrations folder فارغ** | `Infrastructure/Migrations/` | لا توجد أي migration files — يعني القاعدة لم تُنشأ عبر EF migrations أو تم حذفها. |
| 4 | **appsettings.json يحتوي أسرار (secrets)** | `Api/appsettings.json` | JWT Key, SMTP Password (مخفية بنجمات لكن الـ key موجود)، OTP Hashing Secret، Google ClientId — يجب نقلها لـ User Secrets أو Environment Variables. |
| 5 | **appsettings.json غير مُضاف لـ .gitignore** | `.gitignore` | لا يوجد أي استثناء لـ `appsettings.json` أو `appsettings.*.json` في `.gitignore`. |

### 🟡 Medium

| # | المشكلة | الملف/المسار |
|---|---------|-------------|
| 6 | **FileName.cs — ملف مُتبقي (leftover)** | `Application/Features/Accounts/StaffAuth/Commands/StaffGoogleLogin/FileName.cs` — كلاس `internal class FileName` فارغ ومُتبقي من IDE auto-creation. يجب حذفه. |
| 7 | **TestFilesController — كنترولر اختبار في production** | `Api/Controllers/Test/TestFilesController.cs` — يتيح رفع/حذف ملفات بدون أي authorization. يجب إزالته أو تقييده ببيئة Development. |
| 8 | **Unit Tests — كلها stubs فارغة** | `tests/Skill-Loop.UnitTests/**` — كل ملفات الاختبار وكذلك `MockDbContextHelper` كلاسات فارغة. |
| 9 | **Logs/ و uploads/ في المشروع** | `Api/Logs/` و `Api/uploads/` — غير مُضافة للـ `.gitignore`، يمكن أن تتسرب log files أو uploaded files للمستودع. |
| 10 | **TransactionBehavior لا يُفعّل على ICommand (بدون TResponse)** | `TransactionBehavior.cs` — العقد `where TRequest : ICommand<TResponse>` يستبعد `ICommand` (بدون generic). أوامر مثل `ChangePasswordCommand : ICommand` **لن تُلف بـ Transaction تلقائياً**. |

### 🟢 Minor / Convention

| # | المشكلة |
|---|---------|
| 11 | **Connection String تشير لـ "ClinicOS"** — اسم Database من مشروع سابق، يجب تغييره لـ "SkillLoop". |
| 12 | **MailSettings.ClinicName = "Skill-Loop"** — بقايا naming من مشروع clinic. |
| 13 | **OtpPurpose enum** يحتوي `AppointmentBooking`, `ViewAppointments` — قيم من مشروع clinic لا تناسب SkillLoop. |
| 14 | **StaffInvitation.Role comment** — يذكر "Admin, Doctor, Receptionist" — أدوار من مشروع clinic. |
| 15 | **`Skill-Loop.Api.csproj.user`** — ملف user-specific يجب أن يكون في `.gitignore`. |
| 16 | **`using System.Numerics` في AppDbContext و Roles.cs** — using غير مستخدم. |
| 17 | **الـ Skills Controller** لا يرث من `BaseApiController` — كلاس فارغ بدون وراثة. |

---

## 9. Module Gap Analysis (Target vs Current)

بناءً على تحليل SkillLoop Backend المستهدف مقارنة بالكود الموجود:

### ✅ Exists (موجود ومُطبّق)

| Module | الحالة | التفاصيل |
|--------|--------|---------|
| **Auth — Staff Login** | ✅ مكتمل | `StaffLoginCommand`, `StaffGoogleLoginCommand`, `RefreshTokenCommand`, `LogoutCommand` |
| **Auth — Password Management** | ✅ مكتمل | `ChangePassword`, `ForgotPassword`, `ResetPassword` |
| **Users — Account Management** | ✅ مكتمل | `GetAllUsers`, `GetMyProfile`, `UpdateMyProfile`, `UpdateMyProfilePicture`, `ActivateUser`, `DeactivateUser`, `AssignRoleToUser`, `RemoveRoleFromUser` |
| **Auth — Staff Invitations** | ✅ مكتمل | `SendInvitation`, `AcceptInvitation`, `AcceptInvitationWithGoogle`, `ValidateInvitation`, `StaffInvitationCreatedEventHandler` |
| **Permission Management** | ✅ مكتمل | `AssignPermissionToRole`, `RemovePermissionFromRole`, `UpdateRolePermissions`, `GetAllPermissions`, `GetRolePermissions`, `GetAllRolesWithPermissions` |
| **OTP Verification** | ✅ جزئي | كيان `OtpVerification` موجود + `OtpService` في Infrastructure. الـ Features في `Application/Features/Otps/` — **UNCLEAR** (لم يتم فحصها بالتفصيل). |
| **File Storage** | ✅ مكتمل | `LocalFileStorage` + `TestFilesController` |
| **Email Notifications** | ✅ مكتمل | `SmtpEmailSender` + HTML Templates + `IdentityNotificationService` |
| **Infrastructure Core** | ✅ مكتمل | Caching, Logging, CorrelationId, JWT, Hangfire, Outbox Pattern |

### ❌ Missing (غير موجود — مطلوب بناؤه)

| Module | الأولوية | ما يجب بناؤه |
|--------|---------|-------------|
| **Auth — Mobile Users (OTP-based)** | 🔴 عالية | Mobile registration/login عبر OTP + Phone. الأساس موجود (OtpVerification entity) لكن لا يوجد mobile auth flow. |
| **Catalog (Skills)** | 🔴 عالية | كيان `Skill` في Domain. تعبئة الـ stubs الفارغة: `CreateSkill`, `UpdateSkill`, `GetSkillById`, `GetSkills`. إضافة `DeleteSkill`. |
| **Instructor Profile** | 🔴 عالية | كيان `InstructorProfile` مرتبط بالمستخدم. CRUD operations + verification. |
| **Teach (Courses/Sessions)** | 🔴 عالية | كيانات `Course`, `Session`, `Curriculum`. CRUD + حالة النشر + الجدولة. |
| **Booking** | 🟡 متوسطة | كيان `Booking` + حالات الحجز + تأكيد OTP. |
| **Wallet** | 🟡 متوسطة | كيان `Wallet` + `Transaction` + رصيد المعلم والطالب. |
| **Payments** | 🟡 متوسطة | تكامل مع بوابة دفع + كيان `Payment`. |
| **Chat** | 🟡 متوسطة | SignalR أو مكتبة chat + كيانات `Conversation`, `Message`. |
| **Notifications (Push)** | 🟡 متوسطة | Push notifications للموبايل (Firebase FCM). البنية التحتية للبريد موجودة لكن push غير موجود. |
| **Reviews** | 🟢 منخفضة | كيان `Review` + CRUD + حساب التقييم. |

### 📋 Summary Table

```
Module                  | Domain | Application | Infrastructure | API | Tests
------------------------|--------|-------------|----------------|-----|------
Auth (Staff)            |   ✅   |     ✅      |       ✅       |  ✅  |  ⬜
Auth (Mobile)           |   ⬜   |     ⬜      |       ⬜       |  ⬜  |  ⬜
Users/Profile           |   ✅   |     ✅      |       ✅       |  ✅  |  ⬜
Instructor Profile      |   ⬜   |     ⬜      |       ⬜       |  ⬜  |  ⬜
Catalog (Skills)        |   ⬜   |     ⬜*     |       ⬜       |  ⬜* |  ⬜
Teach (Courses)         |   ⬜   |     ⬜      |       ⬜       |  ⬜  |  ⬜
Booking                 |   ⬜   |     ⬜      |       ⬜       |  ⬜  |  ⬜
Wallet                  |   ⬜   |     ⬜      |       ⬜       |  ⬜  |  ⬜
Payments                |   ⬜   |     ⬜      |       ⬜       |  ⬜  |  ⬜
Chat                    |   ⬜   |     ⬜      |       ⬜       |  ⬜  |  ⬜
Notifications (Push)    |   ⬜   |     ⬜      |       ⬜       |  ⬜  |  ⬜
Reviews                 |   ⬜   |     ⬜      |       ⬜       |  ⬜  |  ⬜
Permission Management   |   ✅   |     ✅      |       ✅       |  ✅  |  ⬜
OTP                     |   ✅   |     ⬜      |       ✅       |  ⬜  |  ⬜
File Storage            |   —    |     —       |       ✅       |  ✅  |  ⬜
Email                   |   —    |     —       |       ✅       |  —   |  ⬜

✅ = Implemented   ⬜ = Missing   ⬜* = Stub exists (empty files)
```

---

> **عند طلب بناء feature جديدة**: سأتبع هذا الدليل خطوة بخطوة، مع استخدام الـ patterns الموجودة كمرجع (ChangePassword للـ Commands، GetAllUsers للـ Queries مع Caching + Pagination، StaffInvitation للـ Domain Events).
