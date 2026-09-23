using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Skill_Loop.Application.Common.Abstractions.Identity.Authentication;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Application.Common.Abstractions.Identity.Security;
using Skill_Loop.Application.Common.Abstractions.Identity.Tokens;
using Skill_Loop.Application.Common.Abstractions.Identity.UserManagement;
using Skill_Loop.Infrastructure.Identity.Authentication;
using Skill_Loop.Infrastructure.Identity.Authorization;
using Skill_Loop.Infrastructure.Identity.Security;
using Skill_Loop.Infrastructure.Identity.Tokens;
using Skill_Loop.Infrastructure.Identity.UserManagement;
using Skill_Loop.Infrastructure.Persistence.Data;
using Skill_Loop.Infrastructure.Persistence.IdentityModels;

namespace Skill_Loop.Infrastructure.DependencyInjection;


/// <summary>
/// تسجيل خدمات الهوية والمصادقة (Identity Services)
/// </summary>
public static partial class DependencyInjection
{
    /// <summary>
    /// تسجيل Identity مع ApplicationUser و ApplicationRole،
    /// بالإضافة إلى جميع خدمات المصادقة والصلاحيات المخصصة.
    /// </summary>
    private static IServiceCollection AddIdentityServices(this IServiceCollection services)
    {
        // ==============================================================
        // 1. تسجيل Identity مع ApplicationUser و ApplicationRole
        // ==============================================================
        services
            .AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                // ======================================================
                // إعدادات كلمة المرور (Password Settings)
                // ======================================================
                options.Password.RequireDigit = true;                   // يجب أن تحتوي على رقم
                options.Password.RequireLowercase = true;               // يجب أن تحتوي على حرف صغير
                options.Password.RequireNonAlphanumeric = true;         // يجب أن تحتوي على رمز (!@#$)
                options.Password.RequireUppercase = true;               // يجب أن تحتوي على حرف كبير
                options.Password.RequiredLength = 8;                    // الحد الأدنى للطول 8 حروف
                options.Password.RequiredUniqueChars = 1;               // عدد الأحرف الفريدة المطلوبة

                // ======================================================
                // إعدادات قفل الحساب (Lockout Settings)
                // ======================================================
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);  // مدة القفل 5 دقائق
                options.Lockout.MaxFailedAccessAttempts = 5;                      // 5 محاولات فاشلة
                options.Lockout.AllowedForNewUsers = true;                        // تفعيل القفل للمستخدمين الجدد

                // ======================================================
                // إعدادات المستخدم (User Settings)
                // ======================================================
                options.User.RequireUniqueEmail = true;                // البريد الإلكتروني يجب أن يكون فريداً
                options.User.AllowedUserNameCharacters =               // الأحرف المسموح بها في اسم المستخدم
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

                // ======================================================
                // إعدادات تسجيل الدخول (SignIn Settings)
                // ======================================================
                options.SignIn.RequireConfirmedEmail = true;           // يتطلب تأكيد البريد (اختياري)
                options.SignIn.RequireConfirmedPhoneNumber = false;    // لا يتطلب تأكيد رقم الهاتف
            })
            .AddEntityFrameworkStores<AppDbContext>()                  // ربط Identity مع DbContext الخاص بنا
            .AddDefaultTokenProviders()                                // إضافة موفري التوكن (لإعادة تعيين كلمة المرور)
            .AddRoles<ApplicationRole>();                              // تفعيل إدارة الأدوار

        // ==============================================================
        // 2. تسجيل الخدمات الخاصة بنا (Custom Services)
        // ==============================================================

        // ----- خدمات التوكنات -----
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();

        // ----- خدمات الأمان -----
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IPermissionManagementService, PermissionManagementService>();

        // ----- خدمات المصادقة -----
        services.AddScoped<IStaffAuthService, StaffAuthService>();
        services.AddScoped<IUserAuthService, UserAuthService>();
        services.AddScoped<IAuthService, AuthService>();

        // ----- خدمات إدارة المستخدمين -----
        services.AddScoped<IUserManagementService, UserManagementService>();

        return services;
    }
}