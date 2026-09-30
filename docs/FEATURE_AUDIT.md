# SkillLoop Feature Audit & Gap Analysis

## 1. Feature Matrix

| Feature | Domain (entity + rules) | Commands/Queries | Endpoints | Validators | Tests | Events + Handlers | Cache invalidation | Notifications | Emails | Status |
|---|---|---|---|---|---|---|---|---|---|---|
| **Auth** | ApplicationUser, OtpVerification | RegisterUser, VerifyEmailOtp, UserLogin | AuthController | ✅ | Partial | - | - | - | OTP email | **ناقص**: Social Login (Google/Apple/Facebook) للمستخدمين. |
| **Users/Profile** | ApplicationUser | AccountMgmt | ProfileController, UsersController | ✅ | ✅ | - | - | - | Welcome email | **شبه كامل**: إحصائيات البروفايل (Courses Learning/Teaching, Reviews, Students) ناقصة. |
| **Instructors** | InstructorProfile | CRUD, Queries | InstructorProfilesController | ✅ | ✅ | CourseEnrolled, SessionCompleted | - | - | - | **شبه كامل**: ينقصه Dashboard متكاملة للأرباح والفلاتر. |
| **Courses** | Course | Full CRUD | CoursesController | ✅ | ✅ | CourseEnrolled | ✅ | - | نشر كورس | **كامل** |
| **Sections/Lessons** | Section, Lesson | Full CRUD | CoursesController | ✅ | ✅ | - | - | - | - | **كامل**: تم إضافته مؤخراً |
| **Enrollments** | Enrollment | EnrollInCourse, UpdateLessonProgress | EnrollmentsController | ✅ | ✅ | CourseEnrolled | - | - | الاشتراك في كورس | **شبه كامل**: Endpoint إكمال الدرس ناقص. تتبع إكمال الدروس `3/5 lessons`. |
| **Sessions** | Session | Full CRUD | SessionsController | ✅ | ✅ | - | ✅ | - | تعديل/إلغاء جلسة | **شبه كامل**: Requires approval flag ناقص. Session reviews ناقصة. |
| **Session Materials** | SessionMaterial | Upload, Remove | SessionMaterialsController | ✅ | ✅ | MaterialUploaded | - | - | - | **كامل**: مرفوع على Drive. |
| **Bookings** | Booking | Create, Cancel, ChangeStatus | BookingsController | ✅ | ✅ | BookingCreated, Cancelled | ✅ | ✅ | تأكيد حجز, تذكير | **شبه كامل**: double booking و double refund محمية، لكن إيميلات التذكير والتأكيد ناقصة. ربط الشات بالجلسة ناقص. |
| **Wallet** | UserWallet | GetMyWallet | WalletsController | ✅ | ✅ | CreditInstructorWallet | - | - | - | **ناقص**: Checkout وشحن المحفظة ناقص. |
| **Transactions** | WalletTransaction | GetMyTransactions | WalletsController | ✅ | ✅ | - | - | - | - | **شبه كامل** |
| **Promo Codes** | - | - | - | - | - | - | - | - | تطبيق بروموكود | **غائب تماماً**: يحتاج كيان و Commands. |
| **Checkout/شحن** | - | - | - | - | - | - | - | - | إيصال شراء, فشل | **غائب تماماً**: يحتاج كيان Payment وبوابة دفع وهمية. |
| **Chat** | Conversation, Message | CRUD | ChatController, Hub | ✅ | ❌ | - | - | - | رسالة offline | **شبه كامل**: فلاتر الـ Chat ناقصة، Unread count، ربط المحادثة بجلسة. |
| **Notifications** | Notification | - | - | - | - | - | - | - | - | **غائب تماماً كـ Entity**: يوجد IEmailService لكن لا يوجد Notification Table. |
| **Reviews** | CourseReview, InstructorReview | AddReview | CoursesController, etc. | ✅ | ✅ | - | - | - | تقييم جديد | **شبه كامل**: Session reviews ناقصة. منع تقييم كورس بدون اشتراك. |
| **Bookmarks** | CourseBookmark | ToggleBookmark | CoursesController | ✅ | ✅ | - | - | - | - | **كامل** |
| **Categories** | Category | Full CRUD | CategoriesController | ✅ | ✅ | - | ✅ | - | - | **كامل** |
| **Site Settings** | SiteSettings | Update, Get | SiteSettingsController | ✅ | - | - | - | - | - | **شبه كامل**: نسبة تحويل Credits للعملة ناقصة. |
| **Staff Invitations** | StaffInvitation | Send, Accept | StaffInvitationsController | ✅ | ✅ | - | - | - | دعوة موظف | **شبه كامل**: مدة الصلاحية تحتاج مراجعة. |
| **Admin** | ApplicationUser (Role) | - | UsersController (بعضها) | - | - | - | - | - | - | **شبه كامل** |

## 2. تفاصيل الناقص والأخطاء (Gaps & Bugs)
1. **Promo Codes**: غير موجودة. (يحتاج Entity `PromoCode` + Endpoint `ApplyPromoCode`).
2. **Checkout**: غير موجود. (يحتاج `BuyCredits` و بوابة دفع وهمية + webhook).
3. **Wallet**: لا يوجد طريقة للشحن سوى البيع.
4. **Chat**: ينقصه `Unread Count` والفلاتر وربط المحادثة بجلسة.
5. **Enrollment**: تتبع الدروس المنجزة بنسبة وتناسب ناقص (`POST .../lessons/{lessonId}/complete`).
6. **Teach Dashboard**: أرباح الجلسات والكورسات مجمعة.
7. **Session Reviews**: مفقودة تماماً.
8. **Notifications**: غير موجود كجدول في الداتا بيز.
9. **Emails**: غير مبني كـ Background Job مع Idempotency ومحاولة إعادة (Retry).
10. **Bookings**: يحتاج إلى إرسال إيميل عند (BookingCreated, BookingCancelled) وإشعار للمدرب.

## 3. خطة العمل
- **المرحلة 2**: خدمة الإيميلات بالكامل كـ Background Service باستخدام Outbox.
- **المرحلة 3**: إكمال باقي الـ Features (Checkout, Promo Codes, Notifications Entity, Session Reviews, Chat Enhancements, Enrollment Progress).
- **المرحلة 4**: الـ Unit Tests, Migrations, وتحديث الـ Docs.
