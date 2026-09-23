using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Accounts.UserAuth.Shared;

namespace Skill_Loop.Application.Features.Accounts.UserAuth.Commands.LoginUser;

public sealed record UserLoginCommand(
    string Email,
    string Password) : ICommand<UserAuthResponse>;