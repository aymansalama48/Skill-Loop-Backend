using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Abstractions.Identity.Security;

public interface IPasswordService
{

    Task<Result> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken);

   
    Task<Result<string>> ForgotPasswordAsync(string email, CancellationToken cancellationToken);


    Task<Result> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken);

}
