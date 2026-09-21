namespace Skill_Loop.Application.Features.Accounts.StaffInvitations.Commands.AcceptInvitation;

using Skill_Loop.Application.Common.Abstractions.Messaging;

public sealed record AcceptInvitationCommand(
    string InvitationToken,
    string FullName,
    string Password,
    string? PhoneNumber = null) : ICommand<bool>;