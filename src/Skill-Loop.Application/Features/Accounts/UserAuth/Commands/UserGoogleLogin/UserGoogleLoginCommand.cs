using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Accounts.UserAuth.Shared;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Accounts.UserAuth.Commands.UserGoogleLogin;

[AllowAnonymous]
public sealed record UserGoogleLoginCommand(string IdToken) : ICommand<UserAuthResponse>;