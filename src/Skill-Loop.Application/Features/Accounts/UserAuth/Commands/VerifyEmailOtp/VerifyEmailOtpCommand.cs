using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Accounts.UserAuth.Commands.VerifyEmailOtp;

[AuthenticatedOnly]
public sealed record VerifyEmailOtpCommand(
    string Email,
    string OtpCode) : ICommand<bool>, ICacheInvalidatorCommand
{
    public IReadOnlyCollection<string> CacheKeys => ["users-list"];
}