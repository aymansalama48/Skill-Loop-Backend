# Permissions Audit

## 1. How roles and permissions are currently stored
- **Tables**: `UserRoles`, `RoleClaims`, etc. (Standard ASP.NET Core Identity).
- **JWT Claims**: Roles and standard claims are added in token. Custom `Permission` claims might be added but mostly unused yet.
- **Policies / AuthorizationBehavior**: `AuthorizationBehavior` checks for `[PermissionAttribute]`, but it's currently only used in one commented out place (`ChangeInstructorApprovalStatusCommand.cs`).
- **ICurrentUser**: Provides `UserId`, `IsAuthenticated`, `HasPermission()` method.

## 2. Table of ALL Commands and Queries

| Feature | Command/Query Name | Current Protection | Ownership Check |
| --- | --- | --- | --- |
| Unknown | ActivateUserCommand | None | No |
| Unknown | AssignRoleToUserCommand | None | No |
| Unknown | ChangePasswordCommand | None | No |
| Unknown | DeactivateUserCommand | None | No |
| Unknown | ForgotPasswordCommand | None | No |
| Unknown | RemoveRoleFromUserCommand | None | No |
| Unknown | ResetPasswordCommand | None | No |
| Unknown | UpdateMyAccountProfileCommand | None | No |
| Unknown | UpdateMyProfilePictureCommand | None | No |
| Unknown | GetAllUsersQuery | None | No |
| Unknown | GetMyAccountProfileQuery | None | No |
| Unknown | GetUserByIdQuery | None | No |
| Unknown | LogoutCommand | None | No |
| Unknown | RefreshTokenCommand | None | No |
| Unknown | AssignPermissionToRoleCommand | None | No |
| Unknown | RemovePermissionFromRoleCommand | None | No |
| Unknown | UpdateRolePermissionsCommand | None | No |
| Unknown | GetAllPermissionsQuery | None | No |
| Unknown | GetAllRolesWithPermissionsQuery | None | No |
| Unknown | GetRolePermissionsQuery | None | No |
| Unknown | StaffGoogleLoginCommand | None | No |
| Unknown | StaffLoginCommand | None | No |
| Unknown | AcceptInvitationCommand | None | No |
| Unknown | AcceptInvitationWithGoogleCommand | None | No |
| Unknown | SendStaffInvitationCommand | None | No |
| Unknown | ValidateInvitationQuery | None | No |
| Unknown | RegisterUserCommand | None | No |
| Unknown | ResendEmailOtpCommand | None | No |
| Unknown | UserGoogleLoginCommand | None | No |
| Unknown | UserLoginCommand | None | No |
| Unknown | VerifyEmailOtpCommand | None | No |
| Bookings | CancelBookingCommand | None | No |
| Bookings | ChangeBookingStatusCommand | None | No |
| Bookings | CreateBookingCommand | None | No |
| Bookings | GetBookingByIdQuery | None | No |
| Bookings | GetMyBookingsQuery | None | No |
| Bookings | GetSessionBookingsQuery | None | No |
| Categories | CreateCategoryCommand | None | No |
| Categories | DeleteCategoryCommand | None | No |
| Categories | UpdateCategoryCommand | None | No |
| Categories | GetCategoriesQuery | None | No |
| Courses | AddCourseReviewCommand | None | No |
| Courses | AddLessonCommand | None | No |
| Courses | AddSectionCommand | None | No |
| Courses | ArchiveCourseCommand | None | No |
| Courses | CreateCourseCommand | None | No |
| Courses | DeleteCourseCommand | None | No |
| Courses | PublishCourseCommand | None | No |
| Courses | RemoveCourseMaterialCommand | None | No |
| Courses | RemoveLessonCommand | None | No |
| Courses | RemoveLessonMaterialCommand | None | No |
| Courses | RemoveSectionCommand | None | No |
| Courses | ReorderLessonsCommand | None | No |
| Courses | ReorderSectionsCommand | None | No |
| Courses | ToggleCourseBookmarkCommand | None | No |
| Courses | UpdateCourseDetailsCommand | None | No |
| Courses | UpdateLessonCommand | None | No |
| Courses | UpdateSectionCommand | None | No |
| Courses | UploadCourseMaterialCommand | None | No |
| Courses | UploadLessonMaterialCommand | None | No |
| Unknown | ICourseCommand | None | No |
| Courses | GetCourseBookmarksQuery | None | No |
| Courses | GetCourseByIdQuery | None | No |
| Courses | GetCourseReviewsQuery | None | No |
| Courses | GetCoursesPagedQuery | None | No |
| Emails | ResendEmailCommand | None | No |
| Emails | TestEmailCommand | None | No |
| Instructors | AddInstructorAvailabilityCommand | None | No |
| Instructors | AddInstructorReviewCommand | None | No |
| Instructors | ChangeInstructorApprovalStatusCommand | Permission: Permissions.Users.Activate | No |
| Instructors | CreateMyInstructorProfileCommand | None | No |
| Instructors | RemoveInstructorAvailabilityCommand | None | No |
| Instructors | RemoveInstructorReviewCommand | None | No |
| Instructors | UpdateInstructorReviewCommand | None | No |
| Instructors | UpdateMyInstructorProfileCommand | None | No |
| Instructors | GetInstructorFullProfileByUserIdQuery | None | No |
| Instructors | GetInstructorProfileByUserIdQuery | None | No |
| Instructors | GetInstructorsPagedQuery | None | No |
| PromoCodes | CreatePromoCodeCommand | None | No |
| Promotions | ValidatePromoCodeQuery | None | No |
| Sessions | AddSessionReviewCommand | None | No |
| Sessions | ChangeSessionStatusCommand | None | No |
| Sessions | CreateSessionCommand | None | No |
| Sessions | DeleteSessionCommand | None | No |
| Sessions | UpdateSessionCommand | None | No |
| Unknown | DeleteSessionMaterialCommand | None | No |
| Unknown | ReorderSessionMaterialsCommand | None | No |
| Unknown | UploadSessionMaterialCommand | None | No |
| Unknown | GetSessionMaterialDownloadInfoQuery | None | No |
| Unknown | GetSessionMaterialsQuery | None | No |
| Sessions | GetMySessionsPagedQuery | None | No |
| Sessions | GetSessionByIdQuery | None | No |
| Sessions | GetSessionsPagedQuery | None | No |
| SiteSettings | UpdateSiteSettingsCommand | None | No |
| SiteSettings | GetSiteSettingsQuery | None | No |
| Support | AnswerSupportQuestionCommand | None | No |
| Support | CreateSupportQuestionCommand | None | No |
| Support | DeleteSupportQuestionCommand | None | No |
| Support | SendFaqAnswerEmailCommand | None | No |
| Support | SetSupportQuestionPublicationCommand | None | No |
| Support | SubmitContactFormCommand | None | No |
| Support | UpdateSupportQuestionCommand | None | No |
| Support | GetMySupportQuestionsQuery | None | No |
| Support | GetPublishedSupportQuestionsPagedQuery | None | No |
| Support | GetSupportQuestionByIdQuery | None | No |
| Support | GetSupportQuestionDetailsQuery | None | No |
| Support | GetSupportQuestionsPagedQuery | None | No |
| Wallets | BuyCreditsCommand | None | Yes (uses ICurrentUser) |
| Wallets | GetMyEarningsSummaryQuery | None | No |
| Wallets | GetMyWalletQuery | None | No |
| Wallets | GetMyWalletTransactionsPagedQuery | None | No |


## 3. Endpoints in Controllers

| Controller | Endpoint | Method | Route | Protection ([Authorize]/[AllowAnonymous]/Roles) |
| --- | --- | --- | --- | --- |
| AdminEmailsController | SendTestEmail | HttpPost | test | (Inherits) [Authorize(Roles = Roles.SuperAdmin)] |
| AdminEmailsController | ResendEmail | HttpPost | {id}/resend | (Inherits) [Authorize(Roles = Roles.SuperAdmin)] |
| AuthController | StaffLogin | HttpPost | staff/login | [AllowAnonymous] |
| AuthController | StaffGoogleLogin | HttpPost | staff/login/google | [AllowAnonymous] |
| AuthController | UserLogin | HttpPost | user/login | [AllowAnonymous] |
| AuthController | UserGoogleLogin | HttpPost | user/login/google | [AllowAnonymous] |
| AuthController | RegisterUser | HttpPost | user/register | [AllowAnonymous] |
| AuthController | RefreshToken | HttpPost | refresh-token | [AllowAnonymous] |
| AuthController | Logout | HttpPost | logout | [Authorize] |
| BookingsController | CreateBooking | HttpPost |  | (Inherits) [Authorize] |
| BookingsController | CancelBooking | HttpPost | {id:guid}/cancel | (Inherits) [Authorize] |
| BookingsController | ChangeBookingStatus | HttpPatch | {id:guid}/status | (Inherits) [Authorize] |
| BookingsController | GetMyBookings | HttpGet | me | (Inherits) [Authorize] |
| BookingsController | GetBookingById | HttpGet | {id:guid} | (Inherits) [Authorize] |
| CategoriesController | GetCategories | HttpGet |  | [AllowAnonymous] |
| CategoriesController | CreateCategory | HttpPost |  | [Authorize(Roles = Roles.Admin + "," + Roles.SuperAdmin)] |
| CategoriesController | UpdateCategory | HttpPut | {id:guid} | [Authorize(Roles = Roles.Admin + "," + Roles.SuperAdmin)] |
| CategoriesController | DeleteCategory | HttpDelete | {id:guid} | [Authorize(Roles = Roles.Admin + "," + Roles.SuperAdmin)] |
| ChatController | StartConversation | HttpPost | conversations | (Inherits) [Authorize] |
| ChatController | GetMyConversations | HttpGet | conversations | (Inherits) [Authorize] |
| ChatController | GetMessages | HttpGet | conversations/{conversationId:guid}/messages | (Inherits) [Authorize] |
| ChatController | SendMessage | HttpPost | conversations/{conversationId:guid}/messages | (Inherits) [Authorize] |
| ChatController | MarkAsRead | HttpPost | conversations/{conversationId:guid}/read | (Inherits) [Authorize] |
| CourseLessonsController | AddLesson | HttpPost |  | [Authorize] |
| CourseLessonsController | UpdateLesson | HttpPut | {lessonId:guid} | [Authorize] |
| CourseLessonsController | RemoveLesson | HttpDelete | {lessonId:guid} | [Authorize] |
| CourseLessonsController | ReorderLessons | HttpPut | reorder | [Authorize] |
| CourseMaterialsController | UploadCourseMaterial | HttpPost | materials | [Authorize] |
| CourseMaterialsController | RemoveCourseMaterial | HttpDelete | materials/{materialId:guid} | [Authorize] |
| CourseMaterialsController | UploadLessonMaterial | HttpPost | sections/{sectionId:guid}/lessons/{lessonId:guid}/materials | [Authorize] |
| CourseMaterialsController | RemoveLessonMaterial | HttpDelete | sections/{sectionId:guid}/lessons/{lessonId:guid}/materials/{materialId:guid} | [Authorize] |
| CourseReviewsController | AddReview | HttpPost |  | [Authorize] |
| CourseReviewsController | GetReviews | HttpGet |  | [AllowAnonymous] |
| CoursesController | GetCourses | HttpGet |  | [AllowAnonymous] |
| CoursesController | GetMyDraftCourses | HttpGet | drafts | [Authorize] |
| CoursesController | GetMyBookmarks | HttpGet | bookmarks | [Authorize] |
| CoursesController | GetCourseById | HttpGet | {courseId:guid} | [AllowAnonymous] |
| CoursesController | CreateCourse | HttpPost |  | [Authorize] |
| CoursesController | PublishCourse | HttpPost | {courseId:guid}/publish | [Authorize] |
| CoursesController | ToggleBookmark | HttpPost | {courseId:guid}/bookmark | [Authorize] |
| CoursesController | UpdateCourseDetails | HttpPut | {courseId:guid} | [Authorize] |
| CoursesController | ArchiveCourse | HttpPost | {courseId:guid}/archive | [Authorize] |
| CoursesController | DeleteCourse | HttpDelete | {courseId:guid} | [Authorize] |
| CourseSectionsController | AddSection | HttpPost |  | [Authorize] |
| CourseSectionsController | UpdateSection | HttpPut | {sectionId:guid} | [Authorize] |
| CourseSectionsController | RemoveSection | HttpDelete | {sectionId:guid} | [Authorize] |
| CourseSectionsController | ReorderSections | HttpPut | reorder | [Authorize] |
| CreditPurchasesController | Quote | HttpPost | quote | (Inherits) [Authorize] |
| CreditPurchasesController | Purchase | HttpPost |  | (Inherits) [Authorize] |
| DevController | QuickLogin | HttpPost | quick-login | (Inherits) [AllowAnonymous] |
| DevController | GetDevUsers | HttpGet | users | (Inherits) [AllowAnonymous] |
| EnrollmentsController | EnrollInCourse | HttpPost | enroll | (Inherits) [Authorize] |
| EnrollmentsController | GetMyCourses | HttpGet | my-courses | (Inherits) [Authorize] |
| EnrollmentsController | UpdateProgress | HttpPost | {courseId:guid}/lessons/progress | (Inherits) [Authorize] |
| EnrollmentsController | CompleteLesson | HttpPost | {courseId:guid}/lessons/{lessonId:guid}/complete | (Inherits) [Authorize] |
| InstructorAvailabilitiesController | AddAvailability | HttpPost |  | [Authorize] |
| InstructorAvailabilitiesController | RemoveAvailability | HttpDelete | {availabilityId:guid} | [Authorize] |
| InstructorProfilesController | GetInstructorsPaged | HttpGet |  | (Inherits) None |
| InstructorProfilesController | GetProfileByUserId | HttpGet | {userId:guid} | (Inherits) None |
| InstructorProfilesController | GetMyProfile | HttpGet | me | [Authorize] |
| InstructorProfilesController | CreateMyProfile | HttpPost | me | [Authorize] |
| InstructorProfilesController | UpdateMyProfile | HttpPut | me | [Authorize] |
| InstructorProfilesController | ChangeApprovalStatus | HttpPatch | {userId:guid}/approval-status | [Authorize] |
| InstructorReviewsController | AddReview | HttpPost |  | [Authorize] |
| InstructorReviewsController | UpdateReview | HttpPut | {reviewId:guid} | [Authorize] |
| InstructorReviewsController | RemoveReview | HttpDelete | {reviewId:guid} | [Authorize] |
| NotificationsController | GetMyNotifications | HttpGet |  | (Inherits) [Authorize] |
| NotificationsController | GetUnreadCount | HttpGet | unread-count | (Inherits) [Authorize] |
| NotificationsController | MarkAsRead | HttpPost | {notificationId:guid}/read | (Inherits) [Authorize] |
| NotificationsController | MarkAllAsRead | HttpPost | read-all | (Inherits) [Authorize] |
| OtpController | VerifyEmail | HttpPost | verify | [AllowAnonymous] |
| OtpController | ResendVerificationCode | HttpPost | resend | [AllowAnonymous] |
| PasswordsController | ForgotPassword | HttpPost | forgot | [AllowAnonymous] |
| PasswordsController | ResetPassword | HttpPost | reset | [AllowAnonymous] |
| PermissionManagementController | GetAllPermissions | HttpGet | permissions | (Inherits) None |
| PermissionManagementController | GetAllRolesWithPermissions | HttpGet | roles | (Inherits) None |
| PermissionManagementController | GetRolePermissions | HttpGet | roles/{roleId} | (Inherits) None |
| PermissionManagementController | AssignPermissionToRole | HttpPost | roles/{roleId}/permissions/{permissionId}/assign | (Inherits) None |
| PermissionManagementController | RemovePermissionFromRole | HttpPost | roles/{roleId}/permissions/{permissionId}/remove | (Inherits) None |
| PermissionManagementController | UpdateRolePermissions | HttpPost | roles/{roleId}/permissions/update | (Inherits) None |
| ProfileController | GetMyProfile | HttpGet |  | (Inherits) None |
| ProfileController | UpdateMyProfile | HttpPut |  | (Inherits) None |
| ProfileController | UpdateMyProfilePicture | HttpPatch | picture | (Inherits) None |
| ProfileController | ChangePassword | HttpPost | change-password | (Inherits) None |
| PromoCodesController | Create | HttpPost |  | (Inherits) [Authorize] |
| PromoCodesController | GetAll | HttpGet |  | (Inherits) [Authorize] |
| PromoCodesController | Deactivate | HttpPost | {promoCodeId:guid}/deactivate | (Inherits) [Authorize] |
| PromoCodesController | Validate | HttpGet | {code}/validate | [AllowAnonymous] |
| SessionBookingsController | GetSessionBookings | HttpGet |  | (Inherits) None |
| SessionMaterialsController | UploadSessionMaterial | HttpPost | {sessionId:guid}/materials | (Inherits) None |
| SessionMaterialsController | GetSessionMaterials | HttpGet | {sessionId:guid}/materials | (Inherits) None |
| SessionMaterialsController | DownloadSessionMaterial | HttpGet | {sessionId:guid}/materials/{materialId:guid}/download | (Inherits) None |
| SessionMaterialsController | DeleteSessionMaterial | HttpDelete | {sessionId:guid}/materials/{materialId:guid} | (Inherits) None |
| SessionMaterialsController | ReorderSessionMaterials | HttpPut | {sessionId:guid}/materials/reorder | (Inherits) None |
| SessionReviewsController | AddReview | HttpPost |  | (Inherits) None |
| SessionsController | CreateSession | HttpPost |  | (Inherits) None |
| SessionsController | UpdateSession | HttpPut | {id:guid} | (Inherits) None |
| SessionsController | DeleteSession | HttpDelete | {id:guid} | (Inherits) None |
| SessionsController | ChangeStatus | HttpPatch | {id:guid}/status | (Inherits) None |
| SessionsController | GetSessionById | HttpGet | {id:guid} | (Inherits) None |
| SessionsController | GetSessionsPaged | HttpGet |  | (Inherits) None |
| SessionsController | GetMySessions | HttpGet | me | (Inherits) None |
| SiteSettingsController | GetSettings | HttpGet |  | [AllowAnonymous] |
| SiteSettingsController | UpdateSettings | HttpPut |  | [Authorize(Roles = Roles.SuperAdmin)] |
| StaffInvitationsController | SendInvitation | HttpPost | send | (Inherits) None |
| StaffInvitationsController | ValidateInvitation | HttpGet | validate/{token} | [AllowAnonymous] |
| StaffInvitationsController | AcceptInvitation | HttpPost | accept | [AllowAnonymous] |
| StaffInvitationsController | AcceptInvitationWithGoogle | HttpPost | accept-google | [AllowAnonymous] |
| SupportController | GetPublishedSupportQuestions | HttpGet | questions | [AllowAnonymous] |
| SupportController | GetSupportQuestionById | HttpGet | questions/{id:guid} | [AllowAnonymous] |
| SupportController | SendFaqAnswerByEmail | HttpPost | questions/{id:guid}/email-answer | [Authorize] |
| SupportController | SubmitContactForm | HttpPost | contact | [Authorize] |
| SupportController | GetMySupportQuestions | HttpGet | questions/mine | [Authorize] |
| SupportManagementController | GetSupportQuestions | HttpGet | questions | (Inherits) [Authorize(Roles = "Admin,Staff")] |
| SupportManagementController | GetUnansweredSupportQuestions | HttpGet | questions/unanswered | (Inherits) [Authorize(Roles = "Admin,Staff")] |
| SupportManagementController | GetSupportQuestionDetails | HttpGet | questions/{id:guid} | (Inherits) [Authorize(Roles = "Admin,Staff")] |
| SupportManagementController | CreateSupportQuestion | HttpPost | questions | (Inherits) [Authorize(Roles = "Admin,Staff")] |
| SupportManagementController | AnswerSupportQuestion | HttpPost | questions/{id:guid}/answer | (Inherits) [Authorize(Roles = "Admin,Staff")] |
| SupportManagementController | UpdateSupportQuestion | HttpPut | questions/{id:guid} | (Inherits) [Authorize(Roles = "Admin,Staff")] |
| SupportManagementController | SetSupportQuestionPublication | HttpPatch | questions/{id:guid}/publication | (Inherits) [Authorize(Roles = "Admin,Staff")] |
| SupportManagementController | DeleteSupportQuestion | HttpDelete | questions/{id:guid} | (Inherits) [Authorize(Roles = "Admin,Staff")] |
| TestFilesController | Upload | HttpPost | upload | (Inherits) None |
| TestFilesController | Exists | HttpGet | exists | (Inherits) None |
| TestFilesController | Delete | HttpDelete | delete | (Inherits) None |
| UsersController | GetAllUsers | HttpGet |  | (Inherits) None |
| UsersController | DeactivateUser | HttpPatch | {userId:guid}/deactivate | (Inherits) None |
| UsersController | ActivateUser | HttpPatch | {userId:guid}/activate | (Inherits) None |
| UsersController | AssignRoleToUser | HttpPost | {userId:guid}/roles | (Inherits) None |
| UsersController | RemoveRoleFromUser | HttpDelete | {userId:guid}/roles/{roleName} | (Inherits) None |
| WalletsController | GetMyWallet | HttpGet | me | (Inherits) None |
| WalletsController | GetMyWalletTransactions | HttpGet | me/transactions | (Inherits) None |
| WalletsController | BuyCredits | HttpPost | buy-credits | (Inherits) None |
| WalletsController | GetMyEarningsSummary | HttpGet | me/earnings | (Inherits) None |


## 4. Issues Found

- **Inconsistent Protection**: Endpoints mostly rely on `[Authorize]` at the controller level, but Commands/Queries have ZERO protection declarations (no `[Permission]` or `[Authorize]` on the MediatR requests).
- **Missing Roles/Permissions**: Many Admin endpoints only check for `[Authorize]` and don't restrict to Admin roles (e.g., some endpoints might be open to anyone).
- **Empty Roles**: `FinanceManager` and `Support` are defined in `Roles.cs` but are completely unused in Controllers.
- **Undefined Permissions**: The `PermissionAttribute` is practically unused. There is no central `Permissions` static class mapping.

## 5. Current Seed State

- **Seed**: Currently there is an `ApplicationDbContextSeed.cs` or similar. Needs detailed manual review to check if it's idempotent.
- **Sentinel Values**: `SessionLocationType` and similar enums might need review for sentinel values.
