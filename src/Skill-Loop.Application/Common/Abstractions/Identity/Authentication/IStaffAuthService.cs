using Skill_Loop.Application.Features.Accounts.StaffAuth.Shared;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Abstractions.Identity.Authentication;

public interface IStaffAuthService
{

    Task<Result<StaffAuthResponse>> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task<Result<StaffAuthResponse>> LoginWithGoogleAsync(
        string idToken,
        CancellationToken cancellationToken = default);
}

