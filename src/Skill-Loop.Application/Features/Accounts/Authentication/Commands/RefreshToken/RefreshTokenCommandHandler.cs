using Skill_Loop.Application.Common.Abstractions.Identity.Tokens;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Accounts.StaffAuth.Shared;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Accounts.Authentication.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler(IRefreshTokenService refreshTokenService)
    : ICommandHandler<RefreshTokenCommand, StaffAuthResponse>
{
    public async Task<Result<StaffAuthResponse>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        return await refreshTokenService.RefreshTokenAsync(
            request.RefreshToken,
            cancellationToken);
    }
}