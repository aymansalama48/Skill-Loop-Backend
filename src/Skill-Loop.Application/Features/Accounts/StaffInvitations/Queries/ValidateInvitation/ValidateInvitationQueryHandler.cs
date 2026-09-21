namespace Skill_Loop.Application.Features.Accounts.StaffInvitations.Queries.ValidateInvitation;

using Skill_Loop.Application.Common.Abstractions.Identity.Invitations;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Domain.Common.Results;

// استخدام IQueryHandler<TQuery, TResponse> المخصصة[cite: 16]
public sealed class ValidateInvitationQueryHandler(
    IInvitationService invitationService)
    : IQueryHandler<ValidateInvitationQuery, InvitationDetailsDto>
{
    public async Task<Result<InvitationDetailsDto>> Handle(
        ValidateInvitationQuery request,
        CancellationToken cancellationToken)
    {
        return await invitationService.ValidateInvitationTokenAsync(request.Token, cancellationToken);
    }
}