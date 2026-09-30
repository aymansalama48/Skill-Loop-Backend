using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string basePath = @"..\..\src\Skill-Loop.Application\Features";
        if (!Directory.Exists(basePath)) basePath = @"..\src\Skill-Loop.Application\Features";
        
        var mapping = new Dictionary<string, string>
        {
            // AllowAnonymous
            { "RegisterUserCommand", "[AllowAnonymous]" },
            { "UserLoginCommand", "[AllowAnonymous]" },
            { "StaffLoginCommand", "[AllowAnonymous]" },
            { "UserGoogleLoginCommand", "[AllowAnonymous]" },
            { "StaffGoogleLoginCommand", "[AllowAnonymous]" },
            { "VerifyEmailOtpCommand", "[AllowAnonymous]" },
            { "ResendEmailOtpCommand", "[AllowAnonymous]" },
            { "RefreshTokenCommand", "[AllowAnonymous]" },
            { "ForgotPasswordCommand", "[AllowAnonymous]" },
            { "ResetPasswordCommand", "[AllowAnonymous]" },
            { "ValidateInvitationQuery", "[AllowAnonymous]" },
            { "AcceptInvitationCommand", "[AllowAnonymous]" },
            { "AcceptInvitationWithGoogleCommand", "[AllowAnonymous]" },
            { "GetCategoriesQuery", "[AllowAnonymous]" },
            { "GetCoursesPagedQuery", "[AllowAnonymous]" },
            { "GetCourseByIdQuery", "[AllowAnonymous]" },
            { "GetCourseReviewsQuery", "[AllowAnonymous]" },
            { "GetInstructorsPagedQuery", "[AllowAnonymous]" },
            { "GetInstructorFullProfileByUserIdQuery", "[AllowAnonymous]" },
            { "GetInstructorProfileByUserIdQuery", "[AllowAnonymous]" },
            { "GetSessionsPagedQuery", "[AllowAnonymous]" },
            { "GetSessionByIdQuery", "[AllowAnonymous]" },
            { "GetSiteSettingsQuery", "[AllowAnonymous]" },
            { "SubmitContactFormCommand", "[AllowAnonymous]" },
            { "GetPublishedSupportQuestionsPagedQuery", "[AllowAnonymous]" },
            { "GetSupportQuestionDetailsQuery", "[AllowAnonymous]" },

            // Users
            { "GetAllUsersQuery", "[Permission(Permissions.Users.View)]" },
            { "GetUserByIdQuery", "[Permission(Permissions.Users.View)]" },
            { "ActivateUserCommand", "[Permission(Permissions.Users.Activate)]" },
            { "DeactivateUserCommand", "[Permission(Permissions.Users.Deactivate)]" },
            { "AssignRoleToUserCommand", "[Permission(Permissions.Users.AssignRole)]" },
            { "RemoveRoleFromUserCommand", "[Permission(Permissions.Users.AssignRole)]" },

            // Access
            { "AssignPermissionToRoleCommand", "[Permission(Permissions.Access.PermissionsManage)]" },
            { "RemovePermissionFromRoleCommand", "[Permission(Permissions.Access.PermissionsManage)]" },
            { "UpdateRolePermissionsCommand", "[Permission(Permissions.Access.PermissionsManage)]" },
            { "GetAllPermissionsQuery", "[Permission(Permissions.Access.RolesManage)]" },
            { "GetAllRolesWithPermissionsQuery", "[Permission(Permissions.Access.RolesManage)]" },
            { "GetRolePermissionsQuery", "[Permission(Permissions.Access.RolesManage)]" },
            { "SendStaffInvitationCommand", "[Permission(Permissions.Access.InvitationsSend)]" },

            // Bookings
            { "GetSessionBookingsQuery", "[Permission(Permissions.Bookings.ViewAll)]" },
            { "GetBookingByIdQuery", "[Permission(Permissions.Bookings.ViewAll)]" },
            { "ChangeBookingStatusCommand", "[Permission(Permissions.Bookings.ManageAll)]" },

            // Categories
            { "CreateCategoryCommand", "[Permission(Permissions.Catalog.CategoriesManage)]" },
            { "UpdateCategoryCommand", "[Permission(Permissions.Catalog.CategoriesManage)]" },
            { "DeleteCategoryCommand", "[Permission(Permissions.Catalog.CategoriesManage)]" },

            // Courses
            { "CreateCourseCommand", "[Permission(Permissions.Courses.Create)]" },
            { "AddLessonCommand", "[Permission(Permissions.Courses.Create)]" },
            { "AddSectionCommand", "[Permission(Permissions.Courses.Create)]" },
            { "UpdateCourseDetailsCommand", "[Permission(Permissions.Courses.Update)]" },
            { "UpdateLessonCommand", "[Permission(Permissions.Courses.Update)]" },
            { "UpdateSectionCommand", "[Permission(Permissions.Courses.Update)]" },
            { "ReorderLessonsCommand", "[Permission(Permissions.Courses.Update)]" },
            { "ReorderSectionsCommand", "[Permission(Permissions.Courses.Update)]" },
            { "UploadCourseMaterialCommand", "[Permission(Permissions.Courses.Update)]" },
            { "UploadLessonMaterialCommand", "[Permission(Permissions.Courses.Update)]" },
            { "RemoveCourseMaterialCommand", "[Permission(Permissions.Courses.Update)]" },
            { "RemoveLessonCommand", "[Permission(Permissions.Courses.Update)]" },
            { "RemoveLessonMaterialCommand", "[Permission(Permissions.Courses.Update)]" },
            { "RemoveSectionCommand", "[Permission(Permissions.Courses.Update)]" },
            { "DeleteCourseCommand", "[Permission(Permissions.Courses.Delete)]" },
            { "PublishCourseCommand", "[Permission(Permissions.Courses.Publish)]" },
            { "ArchiveCourseCommand", "[Permission(Permissions.Courses.Archive)]" },

            // Instructors
            { "ChangeInstructorApprovalStatusCommand", "[Permission(Permissions.Instructors.ManageAll)]" },

            // Promos
            { "CreatePromoCodeCommand", "[Permission(Permissions.Finance.PromoCodesManage)]" },

            // Sessions
            { "CreateSessionCommand", "[Permission(Permissions.Sessions.Create)]" },
            { "UpdateSessionCommand", "[Permission(Permissions.Sessions.Update)]" },
            { "DeleteSessionCommand", "[Permission(Permissions.Sessions.Delete)]" },
            { "ChangeSessionStatusCommand", "[Permission(Permissions.Sessions.Moderate)]" },
            { "DeleteSessionMaterialCommand", "[Permission(Permissions.Sessions.DeleteMaterials)]" },
            { "ReorderSessionMaterialsCommand", "[Permission(Permissions.Sessions.ReorderMaterials)]" },
            { "UploadSessionMaterialCommand", "[Permission(Permissions.Sessions.UploadMaterials)]" },
            
            // SiteSettings & Emails
            { "UpdateSiteSettingsCommand", "[Permission(Permissions.SiteSettings.Manage)]" },
            { "ResendEmailCommand", "[Permission(Permissions.SiteSettings.Manage)]" },
            { "TestEmailCommand", "[Permission(Permissions.SiteSettings.Manage)]" },

            // Support
            { "AnswerSupportQuestionCommand", "[Permission(Permissions.Support.Manage)]" },
            { "DeleteSupportQuestionCommand", "[Permission(Permissions.Support.Manage)]" },
            { "SendFaqAnswerEmailCommand", "[Permission(Permissions.Support.Manage)]" },
            { "SetSupportQuestionPublicationCommand", "[Permission(Permissions.Support.Manage)]" },
            { "UpdateSupportQuestionCommand", "[Permission(Permissions.Support.Manage)]" },
            { "GetSupportQuestionsPagedQuery", "[Permission(Permissions.Support.View)]" },
            { "GetSupportQuestionByIdQuery", "[Permission(Permissions.Support.View)]" }
        };

        int modifiedCount = 0;
        foreach (var file in Directory.GetFiles(basePath, "*.cs", SearchOption.AllDirectories))
        {
            string content = File.ReadAllText(file);
            if (content.Contains("[AuthenticatedOnly]"))
            {
                string filename = Path.GetFileNameWithoutExtension(file);
                
                if (mapping.ContainsKey(filename))
                {
                    string replacement = mapping[filename];
                    content = content.Replace("[AuthenticatedOnly]", replacement);
                    
                    // Add using directive if it's not AllowAnonymous and missing Skill_Loop.Domain.Constants
                    if (replacement.Contains("Permissions.") && !content.Contains("using Skill_Loop.Domain.Constants;"))
                    {
                        content = "using Skill_Loop.Domain.Constants;\n" + content;
                    }

                    File.WriteAllText(file, content);
                    Console.WriteLine($"Updated: {filename} -> {replacement}");
                    modifiedCount++;
                }
                else
                {
                    Console.WriteLine($"Kept AuthenticatedOnly: {filename}");
                }
            }
        }
        Console.WriteLine($"Total modified files: {modifiedCount}");
    }
}
