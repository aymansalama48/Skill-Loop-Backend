using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Accounts.StaffAuth.Shared;

namespace Skill_Loop.Application.Features.Accounts.StaffAuth.Commands.StaffLogin;

public sealed record StaffLoginCommand(
    string Email,
    string Password) : ICommand<StaffAuthResponse>;