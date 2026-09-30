using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Accounts.UserAuth.Commands.ResendEmailOtp;

// لا يحتاج مسح كاش
[AuthenticatedOnly]
public sealed record ResendEmailOtpCommand(string Email) : ICommand<bool>;