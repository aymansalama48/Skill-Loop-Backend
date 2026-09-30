namespace Skill_Loop.Application.Features.Accounts.Authentication.Commands.Logout;

using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;


[AuthenticatedOnly]
public sealed record LogoutCommand(
    string RefreshToken) : ICommand<bool>;