using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.ResetPassword;

[AllowAnonymous]
public sealed record ResetPasswordCommand(
    string Email,
    string OtpCode, 
    string NewPassword,
    string ConfirmPassword) : ICommand;