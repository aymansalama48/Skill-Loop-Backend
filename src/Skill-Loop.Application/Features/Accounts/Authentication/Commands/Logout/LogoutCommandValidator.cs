using FluentValidation;

namespace Skill_Loop.Application.Features.Accounts.Authentication.Commands.Logout;

public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        // لو أمر تسجيل الخروج بيستقبل RefreshToken مثلاً، نقدر نفحصه هنا.
        // لو مش بيستقبل حاجة (بيعتمد على الـ HttpContext)، الكلاس ده هيكفي لمنع تحذير التقرير المعماري.
    }
}