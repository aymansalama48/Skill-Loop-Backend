using Microsoft.AspNetCore.Identity;
using Skill_Loop.Application.Common.Abstractions.Identity.Security;
using Skill_Loop.Application.Common.Errors.Identity;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Infrastructure.Persistence.IdentityModels;

namespace Skill_Loop.Infrastructure.Identity.Security;

/// <summary>
/// تنفيذ خدمة إدارة الباسورد — تغيير وإعادة تعيين (معتمدة على OTP)
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

        // UserManager بتاع Identity مش بياخد CancellationToken في الـ Methods بتاعته أصلاً
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
    /// إعادة تعيين الباسورد — يتم استدعاؤها بعد أن يتحقق الـ Handler من كود الـ OTP
    /// </summary>
    public async Task<Result> ResetPasswordAsync(
        string email,
        string newPassword,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);

        // رسالة عامة، متقولش "الإيميل غلط" تحديدًا كحماية أمنية
        if (user is null)
            return Result.Failure(PasswordErrors.ResetFailed);

        // 💡 التريك: نولد التوكن الخاص بـ Identity داخلياً ونستهلكه فوراً لتغيير الباسورد
        // لأن الـ Handler تأكد بالفعل من هوية المستخدم عن طريق الـ OTP المكون من 4 أرقام
        var internalToken = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, internalToken, newPassword);

        return result.Succeeded
            ? Result.Success()
            : Result.Failure(PasswordErrors.ResetFailed);
    }
}