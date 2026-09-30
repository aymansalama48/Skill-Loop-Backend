namespace Skill_Loop.Application.Features.Accounts.StaffInvitations.Queries.ValidateInvitation;

using Skill_Loop.Application.Common.Abstractions.Identity.Invitations;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

// استخدام IQuery<TResponse> المخصصة[cite: 15]
[AllowAnonymous]
public sealed record ValidateInvitationQuery(string Token) : IQuery<InvitationDetailsDto>;