using Skill_Loop.Application.Common.Abstractions.Messaging;

namespace Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.ChangePassword;

/// <summary>
/// أمر تغيير كلمة المرور للمستخدم المسجل حالياً
/// </summary>
public sealed record ChangePasswordCommand(
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword) : ICommand;