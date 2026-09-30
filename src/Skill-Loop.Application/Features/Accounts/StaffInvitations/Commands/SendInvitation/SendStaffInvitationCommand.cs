namespace Skill_Loop.Application.Features.Accounts.StaffInvitations.Commands.SendInvitation;

using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

[Permission(Permissions.Access.InvitationsSend)]
public sealed record SendStaffInvitationCommand(
    string Email,
    string Role) : ICommand<string>;