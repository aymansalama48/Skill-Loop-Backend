namespace Skill_Loop.Application.Features.Accounts.StaffInvitations.Commands.AcceptInvitationWithGoogle;

using Skill_Loop.Application.Common.Abstractions.Identity.Invitations;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Domain.Common.Results;

// استخدام ICommandHandler<TCommand, TResponse> المخصصة[cite: 14]
public sealed class AcceptInvitationWithGoogleCommandHandler(
    IInvitationService invitationService)
    : ICommandHandler<AcceptInvitationWithGoogleCommand, bool>
{
    public async Task<Result<bool>> Handle(
        AcceptInvitationWithGoogleCommand request,
        CancellationToken cancellationToken)
    {
        return await invitationService.AcceptInvitationWithGoogleAsync(
            request.InvitationToken,
            request.GoogleIdToken,
            cancellationToken);
    }
}