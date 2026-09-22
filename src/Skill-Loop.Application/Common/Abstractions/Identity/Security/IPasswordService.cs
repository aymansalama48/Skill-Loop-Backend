using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Abstractions.Identity.Security;

public interface IPasswordService
{

    // تغيير كلمة المرور وأنت مسجل دخول (بكلمة المرور القديمة)
    Task<Result> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken);

    //  Task<Result<string>> ForgotPasswordAsync(string email, CancellationToken cancellationToken);


    // إعادة تعيين كلمة المرور (بعد التأكد من الـ OTP في الـ Handler)
    Task<Result> ResetPasswordAsync(string email, string newPassword, CancellationToken cancellationToken);
}
