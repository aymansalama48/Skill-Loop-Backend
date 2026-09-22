using Skill_Loop.Application.Features.Accounts.StaffAuth.Shared;
using Skill_Loop.Application.Features.Accounts.UserAuth.Shared;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Abstractions.Identity.Authentication;

public interface IUserAuthService
{
    Task<Result<UserAuthResponse>> LoginAsync(
            string email,
            string password,
            CancellationToken cancellationToken = default);

    Task<Result<UserAuthResponse>> LoginWithGoogleAsync(
            string idToken,
            CancellationToken cancellationToken = default);
}