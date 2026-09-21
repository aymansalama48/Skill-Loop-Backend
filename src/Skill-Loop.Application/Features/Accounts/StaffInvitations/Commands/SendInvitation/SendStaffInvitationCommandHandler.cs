namespace Skill_Loop.Application.Features.Accounts.StaffInvitations.Commands.SendInvitation;

using Skill_Loop.Application.Common.Abstractions.Identity.Invitations;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Domain.Common.Results;

public sealed class SendStaffInvitationCommandHandler(
    IInvitationService invitationService)
    : ICommandHandler<SendStaffInvitationCommand, string>
{
    public async Task<Result<string>> Handle(
        SendStaffInvitationCommand request,
        CancellationToken cancellationToken)
    {
        return await invitationService.SendStaffInvitationAsync(
            request.Email,
            request.Role,
            cancellationToken);
    }
}