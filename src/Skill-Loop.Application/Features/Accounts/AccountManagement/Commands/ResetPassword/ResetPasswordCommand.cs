using Skill_Loop.Application.Common.Abstractions.Messaging;

namespace Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.ResetPassword;

public sealed record ResetPasswordCommand(
    string Email,
    string OtpCode, 
    string NewPassword,
    string ConfirmPassword) : ICommand;