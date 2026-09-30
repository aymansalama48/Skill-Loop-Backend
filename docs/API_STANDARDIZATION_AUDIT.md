# API Standardization Audit

## 1. Controllers Overview

| Controller | Endpoints | Responsibilities | Issues |
|---|---|---|---|
| AdminEmailsController | 2 | Admin test/resend emails | - |
| AdminPromoCodesController | 1 | Admin promo code management | Should probably be merged with PromoCodesController or separated to Admin controllers |
| AuthController | 11 | Login, Register, OTP, Password Reset | Contains too many sub-resources (Password, OTP). Could be split into AuthController, PasswordsController, OtpController. |
| BookingsController | 6 | Session bookings | Endpoints for Me vs Global are mixed. |
| CategoriesController | 4 | Course Categories CRUD | - |
| ChatController | 5 | Messaging and SignalR triggers | - |
| CoursesController | 24 | Course CRUD, Lessons, Sections, Bookmarks, Reviews | **God Controller**. Needs to be split into CourseSectionsController, CourseLessonsController, CourseReviewsController, etc. |
| CreditPurchasesController | 2 | Buy credits via Stripe/Paypal Mock | Should probably be part of WalletsController. |
| DevController | 2 | DB seeding for Dev | - |
| EnrollmentsController | 4 | Course Enrollments and Progress | - |
| InstructorProfilesController | 11 | Profiles, Availability, Reviews | Mixed concerns (Reviews, Availability). Can be split. |
| NotificationsController | 4 | In-App Notifications | - |
| PermissionManagementController | 6 | Roles and Permissions | - |
| ProfileController | 4 | Current User Profile | - |
| PromoCodesController | 4 | Promo Codes | Contains Admin logic and validate logic. |
| SessionMaterialsController | 5 | Session File uploads/downloads | Should be related to Sessions. |
| SessionsController | 8 | Session CRUD | - |
| SiteSettingsController | 2 | Global Settings | - |
| StaffInvitationsController | 4 | Staff Invitations | - |
| SupportController | 5 | Public Support/Contact Forms | - |
| SupportManagementController | 8 | Admin Support Management | - |
| UsersController | 5 | Admin User Management | - |
| WalletsController | 3 | User Wallets & Transactions | - |

## 2. Arabic Messages
- **ResultExtensions.cs**: Contains hardcoded Arabic messages for fallback HTTP error codes (e.g. Validation Error -> حدث خطأ في البيانات المدخلة).
- **Domain Entities & Application Handlers**: Numerous classes (like UserWallet, Course, Notification) instantiate `new Error(...)` with Arabic descriptions inline.
- **FluentValidation**: Uses inline Arabic strings or English strings inconsistently.

## 3. Inline `new Error(...)` Usage
There are 50+ instances of `new Error(...)` spread across Domain Entities and Application Features. These must be moved to central `Common/Errors/` static classes.

## 4. Response Format Consistency
- **Success**: Uses `{ data: ... }` or `{ message: ... }`. Needs to be standardized to `{ data: ..., message: ... }` uniformly or stick to `{ data: ... }` for payload and 204 for empty.
- **Error**: Uses `ProblemDetails` via `ResultExtensions.MapError`, but some Handlers might be throwing raw exceptions. `DbUpdateConcurrencyException` is not caught universally as 409.
- **Paging**: Paged lists return `{ items, pageNumber, pageSize, totalCount, totalPages, hasNext, hasPrevious }`. This is mostly standard, but needs to be verified in endpoints.
