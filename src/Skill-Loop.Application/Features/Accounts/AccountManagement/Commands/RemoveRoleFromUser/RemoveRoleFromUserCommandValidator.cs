using FluentValidation;

namespace Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.RemoveRoleFromUser;

public sealed class RemoveRoleFromUserCommandValidator : AbstractValidator<RemoveRoleFromUserCommand>
{
    public RemoveRoleFromUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب.")
            .NotEqual(Guid.Empty).WithMessage("معرف المستخدم غير صالح.");

        RuleFor(x => x.RoleName)
            .NotEmpty().WithMessage("اسم الدور (Role) مطلوب.")
            .MaximumLength(50).WithMessage("اسم الدور لا يمكن أن يتجاوز 50 حرفاً.");
    }
}