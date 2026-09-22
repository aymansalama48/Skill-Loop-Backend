using Skill_Loop.Application.Common.Abstractions.Messaging;

namespace Skill_Loop.Application.Features.Accounts.UserAuth.Commands.ResendEmailOtp;

// لا يحتاج مسح كاش
public sealed record ResendEmailOtpCommand(string Email) : ICommand<bool>;