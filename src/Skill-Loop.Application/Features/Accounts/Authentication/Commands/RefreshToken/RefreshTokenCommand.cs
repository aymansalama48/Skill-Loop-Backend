using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Accounts.StaffAuth.Shared;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Accounts.Authentication.Commands.RefreshToken;

[AuthenticatedOnly]
public sealed record RefreshTokenCommand(
    string RefreshToken) : ICommand<StaffAuthResponse>;