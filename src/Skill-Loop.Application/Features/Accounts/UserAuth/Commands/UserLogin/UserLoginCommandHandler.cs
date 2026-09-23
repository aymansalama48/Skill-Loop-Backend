using Skill_Loop.Application.Common.Abstractions.Identity.Authentication;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Accounts.UserAuth.Commands.LoginUser;
using Skill_Loop.Application.Features.Accounts.UserAuth.Shared;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Accounts.UserAuth.Commands.UserLogin;

public sealed class UserLoginCommandHandler : ICommandHandler<UserLoginCommand, UserAuthResponse>
{
    private readonly IUserAuthService _userAuthService;

    public UserLoginCommandHandler(IUserAuthService userAuthService)
    {
        _userAuthService = userAuthService;
    }

    public async Task<Result<UserAuthResponse>> Handle(
        UserLoginCommand request,
        CancellationToken cancellationToken)
    {
        return await _userAuthService.LoginAsync(
            request.Email,
            request.Password,
            cancellationToken);
    }
}