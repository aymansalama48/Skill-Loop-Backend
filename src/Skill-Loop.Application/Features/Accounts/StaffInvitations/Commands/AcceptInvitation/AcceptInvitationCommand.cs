namespace Skill_Loop.Application.Features.Accounts.StaffInvitations.Commands.AcceptInvitation;

using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

[AuthenticatedOnly]
public sealed record AcceptInvitationCommand(
    string InvitationToken,
    string FullName,
    string Password,
    string? PhoneNumber = null) : ICommand<bool>;