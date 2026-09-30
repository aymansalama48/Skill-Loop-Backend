namespace Skill_Loop.Application.Features.Accounts.StaffInvitations.Commands.AcceptInvitationWithGoogle;

using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

// استخدام ICommand<TResponse> المخصصة[cite: 13]
[AllowAnonymous]
public sealed record AcceptInvitationWithGoogleCommand(
    string InvitationToken,
    string GoogleIdToken) : ICommand<bool>;