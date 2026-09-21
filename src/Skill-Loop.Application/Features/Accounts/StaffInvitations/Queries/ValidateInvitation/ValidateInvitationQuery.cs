namespace Skill_Loop.Application.Features.Accounts.StaffInvitations.Queries.ValidateInvitation;

using Skill_Loop.Application.Common.Abstractions.Identity.Invitations;
using Skill_Loop.Application.Common.Abstractions.Messaging;

// استخدام IQuery<TResponse> المخصصة[cite: 15]
public sealed record ValidateInvitationQuery(string Token) : IQuery<InvitationDetailsDto>;