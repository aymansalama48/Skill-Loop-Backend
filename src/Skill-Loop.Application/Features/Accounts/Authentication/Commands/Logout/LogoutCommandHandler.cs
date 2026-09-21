using Skill_Loop.Application.Common.Abstractions.Identity.Authentication;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Accounts.Authentication.Commands.Logout;

public sealed class LogoutCommandHandler(IStaffAuthService staffAuthService)
    : ICommandHandler<LogoutCommand, bool>
{
    public async Task<Result<bool>> Handle(
        LogoutCommand request,
        CancellationToken cancellationToken)
    {
        return await staffAuthService.LogoutAsync(
            request.RefreshToken,
            cancellationToken);
    }
}