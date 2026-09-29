# Support Subsystem — FAQ + Contact Requests

> **Stack**: Clean Architecture · CQRS (MediatR) · EF Core · Redis cache · SMTP (MailKit)

## 1. الفكرة

العميل عنده سؤال. بيدوّر في الـ FAQ الأول؛ لو لقى إجابته يطلب تبعتله على الإيميل. لو مالقى، بيبعت استفسار جديد وهو **مسجّل دخول**. الفريق بيستقبل إيميل، بيرد، والرد بيتبعت للعميل أوتوماتيك، والسؤال بينزل في الـ FAQ لو اتنشر.

```
                    ┌─ لقى الإجابة؟ ──► POST questions/{id}/email-answer ──► إيميل للإجابة
                    │
عميل ──► GET /questions (Anonymous)
                    │
                    └─ ملقاش ──► POST /contact (Auth) ──► صف unanswered
                                        │                        │
                                   إيميل "استلمنا             إيميل "استفسار جديد"
                                   وسيرد قريباً"              لفريق الدعم
                                                                 │
                                          POST /manage/questions/{id}/answer
                                                                 │
                                              إيميل الرد ──► AnsweredAt + EmailSent
                                                                 │
                                          PATCH .../publication ──► منشور في الـ FAQ
```

## 2. الجداول

| Table | أهم الأعمدة | ملاحظات |
|---|---|---|
| `SupportQuestions` | `Question(≤2000)`, `Answer(≤5000)`, `Category(≤100)`, `UserEmail`, `UserName`, `AskedByUserId`, `IsPublished`, `IsAnswered`, `AnsweredAt`, `EmailSent`, `EmailSentAt` | Index على `Category`, `IsPublished`, `IsAnswered`, `AskedByUserId`, `CreatedAt`. `Id = Guid.CreateVersion7()` |

**قواعد الـ Domain** (كلها بترجع `Result` مش Exceptions):

| Method | السلوك |
|---|---|
| `Create(q, cat, email?, name?, userId?)` | سؤال جديد: `IsPublished = IsAnswered = false` |
| `CreatePublished(q, a, cat)` | لازم إجابة، وبيعمل `IsAnswered = true` |
| `SetAnswer(a)` | بيرفض الإجابة الفاضية؛ بيضبط `IsAnswered` + `AnsweredAt` |
| `Publish()` | **بيفشل بـ `SupportQuestion.NoAnswer` لو مفيش إجابة** |
| `Unpublish()` | بيخفي السؤال من الـ FAQ العام |
| `MarkEmailSent()` | بيختم `EmailSent` + `EmailSentAt` بعد إرسال الإيميل بنجاح |
| `Update(q, a, cat, published)` | تعديل كامل؛ **مش بغيّر `AnsweredAt` لو السؤال متجاوب عليه قبل كده** |

## 3. REST Endpoints — `api/support`

### 3.1 عام (Anonymous) — `SupportController`

| Method | Route | الوظيفة |
|---|---|---|
| GET | `/questions?searchTerm=&category=&pageNumber=&pageSize=` | البحث في الـ FAQ. **المنشور بس** |
| GET | `/questions/{id}` | سؤال منشور واحد |

الاستعلامات دي بترجّع `SupportQuestionResponse` — **من غير `UserEmail` ولا `UserName`**. شرط `IsPublished = true` مكتوب جوه الـ Handler ومش بييجي من الـ Client.

### 3.2 مستخدم مسجّل — `SupportController`

| Method | Route | الوظيفة |
|---|---|---|
| POST | `/contact` | استفسار جديد. Body: `{ "subject", "message", "category" }` |
| GET | `/questions/mine` | استفسارات المستخدم نفسه + حالة كل واحد + الرد |
| POST | `/questions/{id}/email-answer` | ابعت إجابة سؤال منشور على إيميل المستخدم |

> ⚠️ `POST /contact` كان Anonymous في أول نسخة وبيقبل `name` و`email` في الـ Body. اتشال:
> الـ Endpoint بقى `[Authorize]` والاسم/الإيميل بيتقرؤا من `ICurrentUser` (الجWT)،
> عشان محدش يقدر يبعت استفسار باسم حد تاني أو على إيميل حد تاني.

### 3.3 فريق الدعم — `SupportManagementController` (`[Authorize(Roles = "Admin,Staff")]`)

| Method | Route | الوظيفة |
|---|---|---|
| GET | `/manage/questions?isAnswered=&isPublished=` | كل الاستفسارات + بيانات صاحب الاستفسار |
| GET | `/manage/questions/unanswered` | طابور المستني رد |
| GET | `/manage/questions/{id}` | تفاصيل كاملة (منها `UserEmail`, `EmailSent`) |
| POST | `/manage/questions/{id}/answer` | Body: `{ "answer", "publish" }` — بيرد + يبعت الإيميل |
| PUT | `/manage/questions/{id}` | تعديل كامل |
| PATCH | `/manage/questions/{id}/publication` | نشر / إلغاء نشر |
| DELETE | `/manage/questions/{id}` | حذف |

## 4. الإيميلات — `SupportNotificationService`

| الدالة | المستدعية | الإيميل المرسل |
|---|---|---|
| `SendContactFormConfirmationAsync` | `SubmitContactForm` | للمستخدم: استلمنا وسيرد خلال 24-48 ساعة |
| `SendSupportTeamNotificationAsync` | `SubmitContactForm` | لـ `SiteSettings.SupportEmail`: استفسار جديد |
| `SendAnswerNotificationAsync` | `AnswerSupportQuestion` | للمستخدم: رد الفريق |
| `SendFaqAnswerAsync` | `SendFaqAnswerEmail` | للمستخدم: إجابة من الـ FAQ |

القوالب في `Infrastructure/External/Email/EmailTemplates/` (Embedded Resources، عربي RTL):
`ContactFormConfirmation.html`, `SupportNotification.html`, `AnswerNotification.html`.

**قواعد الموثوقية:**
- الإيميلات **متعملش الـ request يفشل**. لو SMTP وقع، الرد بيتسجل في الـ logs والـ request بيرجع 200.
- بنستخدم `CancellationToken.None` للإيميل: لو العميل قفل الصفحة في نص الـ request الإيميل يفضل يتبعت.
- مفيش `Task.Run` fire-and-forget — كل الإرسال متعمل awaited جوّه الـ Handler.
- لو الإيميل فشل بعد حفظ الرد، `EmailSent` بيفضل `false` فالفريق يقدر يعيد الإرسال من غير ما يخسر الرد.

## 5. الكاش

الاستعلامات دي `ICacheableQuery` (10 دقايق sliding / ساعة absolute):

| الاستعلام | المفتاح |
|---|---|
| `GetPublishedSupportQuestionsPaged` | `support:questions:page:...` |
| `GetSupportQuestionsPaged` (إدارة) | `support:questions:manage:page:...` |
| `GetSupportQuestionById` | `support:question:{id}` |
| `GetMySupportQuestions` | `support:my-questions:{userId}:...` |

كل الـ Commands بتنفّذ `ICacheInvalidatorCommand` وبترجّع `SupportCacheKeys.All`، وده بيوصل لـ `RemoveByPrefixAsync` — فبادئة واحدة بتمسح كل الصفحات والبحثات والتصنيفات مرة واحدة. البادئات في `Features/Support/Share/SupportCacheKeys.cs`.

## 6. تصميم SOLID

| المبدأ | التطبيق |
|---|---|
| **SRP** | الكنترولرات مقسومة بالجمهور: `SupportController` (عام + مستخدم) و `SupportManagementController` (فريق الدعم). كل Command/Query في فولدر لوحده. |
| **OCP** | إضافة إيميل جديد = دالة جديدة في `SupportNotificationService`؛ مفيش دالة موجودة بتتعدّل. إضافة بريد أو قناة جديدة = implementation جديد للـ interface. |
| **LSP** | مفيش وراثة بين الـ DTOs (`SupportQuestionResponse`, `SupportQuestionDetailsResponse`, `MySupportQuestionResponse`) — كل واحد shape مختلف حسب الجمهور. `ICacheableQuery<T>` بيشتق من `IQuery<T>` وبيتوافق معاه. |
| **ISP** | إشعارات الدعم مقسومة: `ISupportRequestNotifier` (استلام الاستفسار) و `ISupportAnswerNotifier` (الإجابات). كل Handler بياخد اللي بيحتاجه بس. التنفيذ واحد للاثنين. |
| **DIP** | الـ Handlers بتعتمد على `IApplicationDbContext`, `ICurrentUser`, `ISupportRequestNotifier`/`ISupportAnswerNotifier`, `ILogger<T>` — كلهم abstractions. مفيش أي `new` ولا concrete type. |

> `EmailTemplateEngine` بيتحقن كـ concrete class — بس ده نفس pattern المستخدم في `IdentityNotificationService`، فمقصود.

## 7. الأخطاء

| Code | النوع | المعنى |
|---|---|---|
| `SupportQuestion.EmptyQuestion` | Validation | السؤال فاضي |
| `SupportQuestion.EmptyCategory` | Validation | التصنيف فاضي |
| `SupportQuestion.EmptyAnswer` | Validation | الإجابة فاضية |
| `SupportQuestion.NoAnswer` | Validation | حاول ينشر سؤال من غير إجابة |
| `SupportQuestion.NotFound` | NotFound | السؤال مش موجود (أو مش منشور من ناحية الـ API العام) |
| `SupportQuestion.AnswerNotAvailable` | NotFound | مفيش إجابة متاحة للإرسال |
| `SupportQuestion.Unauthenticated` | Unauthorized | حاول يبعت استفسار من غير توكن |
| `SupportQuestion.MissingIdentifier` | Unauthorized | التوكن مفيهوش `UserId` |
| `SupportNotification.EmailFailed` | Failure | فشل إرسال الإيميل (بينتسجل، مش بيرمي exception) |

## 8. الاختبارات

`tests/Skill-Loop.UnitTests/Features/Support/` — 63 اختبار:

| الملف | التغطية |
|---|---|
| `SupportQuestionTests.cs` | كل قواعد الـ Domain + الحالات الفاشلة |
| `SupportCacheInvalidationTests.cs` | كل Command بيرجّع بادئات الكاش الثلاثة |
| `Commands/SubmitContactForm/…Tests.cs` | الرفض بدون توكن، استخراج الهوية من الـ JWT، الإيميلات، نجاح الطلب حتى لو الإيميل وقع |
| `Commands/AnswerSupportQuestion/…Tests.cs` | الرد، النشر، `MarkEmailSent`، وبقاء الرد لو الإيميل وقع |
| `Commands/SendFaqAnswerEmail/…Tests.cs` | الرفض لو السؤال مش منشور أو متجاوبش |
| `Commands/SetSupportQuestionPublication/…Tests.cs` | رفض النشر من غير إجابة + طابور unanswered |
| `Queries/GetPublishedSupportQuestionsPaged/…Tests.cs` | **المنشور بس** في اللستة والتفاصيل (حماية الـ Anonymous) |
| `Queries/GetMySupportQuestions/…Tests.cs` | كل مستخدم بيشوف استفساراته هو بس |

`tests/Skill-Loop.UnitTests/External/Notifications/SupportNotificationContractsTests.cs` — ثوابت ISP/DIP بالـ reflection.

## 9. Migrations

| Migration | التغيير |
|---|---|
| `20260929161728_AddSupportQuestions` | إنشاء الجدول + الـ indexes |
| `20260929164859_AddAskedByUserIdToSupportQuestions` | عمود `AskedByUserId` + الـ index بتاعه |

## 10. Frontend Contract Notes

- `GET /api/support/questions` مبيرجّعش `userEmail`/`userName` أبداً — لو الواجهة محتاجة تعرض بيانات صاحب الاستفسار، ده endpoint تاني محمي.
- استفسار المستخدم الأول في الـ Body بيتبعت كـ `"[{subject}] {message}"` (same row، مش صفّين).
- `POST /manage/questions/{id}/answer` بيرجّع الـ `id` مش `204`، عشان الواجهة تقدر تعمل optimistic update.
- لو الإيميل فشل، الاستجابة **لسه 200** — مفيش indication في الـ response. لو الواجهة عايزة تعرض حالة "الإيميل مبعتش" لازم تقرأ `emailSent` من `GET /manage/questions/{id}`.
