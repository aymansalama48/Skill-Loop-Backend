using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Accounts.UserAuth.Shared;

namespace Skill_Loop.Application.Features.Accounts.UserAuth.Commands.UserGoogleLogin;

public sealed record UserGoogleLoginCommand(string IdToken) : ICommand<UserAuthResponse>;