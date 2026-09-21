namespace Skill_Loop.Application.Features.Accounts.StaffInvitations.Commands.AcceptInvitation;

using Skill_Loop.Application.Common.Abstractions.Identity.Invitations;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Domain.Common.Results;

// استخدام ICommandHandler<TCommand, TResponse> المخصصة[cite: 14]
public sealed class AcceptInvitationCommandHandler(
    IInvitationService invitationService)
    : ICommandHandler<AcceptInvitationCommand, bool>
{
    public async Task<Result<bool>> Handle(
        AcceptInvitationCommand request,
        CancellationToken cancellationToken)
    {
        return await invitationService.AcceptInvitationAndCreateAccountAsync(
            request.InvitationToken,
            request.FullName,
            request.Password,
            request.PhoneNumber,
            cancellationToken);
    }
}