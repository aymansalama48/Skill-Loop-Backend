namespace Skill_Loop.Application.Features.Accounts.StaffInvitations.Commands.SendInvitation;

using Skill_Loop.Application.Common.Abstractions.Messaging;

public sealed record SendStaffInvitationCommand(
    string Email,
    string Role) : ICommand<string>;