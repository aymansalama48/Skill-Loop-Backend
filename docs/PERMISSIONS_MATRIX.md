# Permissions Matrix

هذا الجدول يوضح التخطيط الكامل للصلاحيات (Permissions) على جميع العمليات داخل النظام، وكيفية حمايتها بالاعتماد على MediatR Behaviors بدلاً من فحص الأدوار (Roles) في مستوى الـ Controllers.

## 1. الأوسمة المستخدمة (Attributes)
- \`[AllowAnonymous]\`: متاح للجميع دون تسجيل دخول.
- \`[AuthenticatedOnly]\`: يحتاج فقط أن يكون المستخدم مسجلاً، وتُطبق قيود الملكية (Ownership) إذا لزم الأمر.
- \`[Permission(Permissions.*)]\`: يحتاج لامتلاك صلاحية معينة صريحة للقيام بالعملية.

## 2. جدول العمليات (Commands & Queries)

| القسم (Feature) | العملية | مستوى الحماية | الملكية (Ownership Check) |
| --- | --- | --- | --- |
| **Accounts** | LoginUser | `[AllowAnonymous]` | - |
| | RegisterUser | `[AllowAnonymous]` | - |
| | VerifyEmailOtp | `[AllowAnonymous]` | - |
| | ChangePassword | `[AuthenticatedOnly]` | نعم |
| | GeneratStaffInvitation | `[Permission(Permissions.Access.InvitationsSend)]` | - |
| **Bookings** | CreateBooking | `[AuthenticatedOnly]` | - |
| | CancelBooking | `[AuthenticatedOnly]` | نعم (الطالب أو المدرب) |
| | GetMyBookings | `[AuthenticatedOnly]` | نعم |
| | ChangeBookingStatus | `[Permission(Permissions.Bookings.ManageAll)]` | - |
| **Categories** | CreateCategory | `[Permission(Permissions.Catalog.CategoriesManage)]` | - |
| | UpdateCategory | `[Permission(Permissions.Catalog.CategoriesManage)]` | - |
| | GetCategoryById | `[AllowAnonymous]` | - |
| **Courses** | CreateCourse | `[Permission(Permissions.Courses.Create)]` | يُنفذ بواسطة Instructor |
| | UpdateCourseDetails | `[Permission(Permissions.Courses.Update)]` | CourseOwnershipBehavior |
| | DeleteCourse | `[Permission(Permissions.Courses.Delete)]` | CourseOwnershipBehavior |
| | PublishCourse | `[Permission(Permissions.Courses.Publish)]` | CourseOwnershipBehavior |
| | ArchiveCourse | `[Permission(Permissions.Courses.Archive)]` | CourseOwnershipBehavior |
| | GetCoursesPaged | `[AllowAnonymous]` | - |
| | GetMyDraftCourses | `[AuthenticatedOnly]` | نعم |
| **Sessions** | CreateSession | `[Permission(Permissions.Sessions.Create)]` | يُنفذ بواسطة Instructor |
| | UpdateSession | `[Permission(Permissions.Sessions.Update)]` | SessionOwnershipBehavior |
| | DeleteSession | `[Permission(Permissions.Sessions.Delete)]` | SessionOwnershipBehavior |
| | ApproveSession | `[Permission(Permissions.Sessions.Moderate)]` | - |
| | UploadSessionMaterial| `[Permission(Permissions.Sessions.UploadMaterials)]`| SessionOwnershipBehavior |
| **Wallets** | BuyCredits | `[AuthenticatedOnly]` | نعم |
| | GetMyWallet | `[AuthenticatedOnly]` | نعم |
| | GetMyEarningsSummary| `[AuthenticatedOnly]` | نعم |
| | GetWalletSummary | `[Permission(Permissions.Finance.View)]` | - |
| **Instructors**| UpdateInstructorProfile| `[AuthenticatedOnly]` | نعم |
| | ChangeInstructorApproval|`[Permission(Permissions.Users.Activate)]` | - |
| **Support** | CreateSupportTicket | `[AuthenticatedOnly]` | نعم |
| | AnswerSupportTicket | `[Permission(Permissions.Support.Manage)]` | - |
| | GetAllTickets | `[Permission(Permissions.Support.View)]` | - |

## 3. ملاحظات إضافية
- تم إزالة `[Authorize(Roles="...")]` من الـ Controllers، والاعتماد بالكامل على `AuthorizationBehavior` في MediatR.
- يتم تعيين الصلاحيات للـ Roles الأساسية (`Admin`، `FinanceManager`، إلخ) عن طريق الـ Seed في `RolePermissionsMap.cs`.
- المدربون والطلاب يعتمدون على الصلاحيات الافتراضية المقيدة بملكية المصدر (`CourseOwnershipBehavior` و `SessionOwnershipBehavior`).
