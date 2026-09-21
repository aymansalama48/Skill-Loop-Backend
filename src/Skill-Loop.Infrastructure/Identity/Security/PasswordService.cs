using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Skill_Loop.Application.Common.Abstractions.Identity.Security;
using Skill_Loop.Application.Common.Errors.Identity;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Infrastructure.Persistence.IdentityModels;
using System.Text;

namespace Skill_Loop.Infrastructure.Identity.Security;

/// <summary>
/// تنفيذ خدمة إدارة الباسورد — تغيير/استعادة/إعادة تعيين
/// </summary>
public class PasswordService(
    UserManager<ApplicationUser> userManager) : IPasswordService
{
    /// <summary>
    /// تغيير الباسورد — بيتحقق من الباسورد الحالي الأول
    /// </summary>
    public async Task<Result> ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Result.Failure(PasswordErrors.UserNotFound);

        // UserManager بتاع Identity مش بياخد CancellationToken في الـ Methods بتاعته أصلاً (قيد في المكتبة نفسها)
        var result = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);

        if (!result.Succeeded)
        {
            // لو الغلط تحديدًا "الباسورد الحالي غلط" بنرجّع Error مخصوص، غير كده Error عام
            var isWrongCurrentPassword = result.Errors
                .Any(e => e.Code == nameof(IdentityErrorDescriber.PasswordMismatch));

            return Result.Failure(isWrongCurrentPassword
                ? PasswordErrors.IncorrectCurrentPassword
                : PasswordErrors.ChangeFailed);
        }

        return Result.Success();
    }

    /// <summary>
    /// طلب استعادة الباسورد — بيرجع توكن الاستعادة (مع حماية Email Enumeration)
    /// </summary>
    public async Task<Result<string>> ForgotPasswordAsync(
            string email,
            CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);

        // حماية Email Enumeration: نرجع Success بقيمة فارغة
        if (user is null)
            return Result<string>.Success(string.Empty);

        var rawToken = await userManager.GeneratePasswordResetTokenAsync(user);

        // ترميز الـ Token
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));

        // ✅ إرجاع التوكن المرمّز بداخل الـ Result
        return Result<string>.Success(encodedToken);
    }

    /// <summary>
    /// إعادة تعيين الباسورد بالتوكن المرسل — برسائل عامة متقولش تفاصيل عن الإيميل
    /// </summary>
    public async Task<Result> ResetPasswordAsync(
        string email,
        string token,
        string newPassword,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return Result.Failure(PasswordErrors.ResetFailed); // نفس رسالة عامة، متقولش "الإيميل غلط" تحديدًا

        string decodedToken;
        try
        {
            decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        }
        catch
        {
            return Result.Failure(PasswordErrors.ResetFailed);
        }

        var result = await userManager.ResetPasswordAsync(user, decodedToken, newPassword);

        return result.Succeeded
            ? Result.Success()
            : Result.Failure(PasswordErrors.ResetFailed);
    }
}