using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Accounts.StaffAuth.Shared;

namespace Skill_Loop.Application.Features.Accounts.Authentication.Commands.RefreshToken;

public sealed record RefreshTokenCommand(
    string RefreshToken) : ICommand<StaffAuthResponse>;