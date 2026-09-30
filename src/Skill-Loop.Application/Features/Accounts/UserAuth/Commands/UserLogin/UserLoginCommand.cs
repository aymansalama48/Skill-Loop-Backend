using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Accounts.UserAuth.Shared;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Accounts.UserAuth.Commands.LoginUser;

[AllowAnonymous]
public sealed record UserLoginCommand(
    string Email,
    string Password) : ICommand<UserAuthResponse>;