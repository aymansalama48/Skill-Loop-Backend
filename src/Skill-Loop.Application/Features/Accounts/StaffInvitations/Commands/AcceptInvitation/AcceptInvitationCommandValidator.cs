namespace Skill_Loop.Application.Features.Accounts.StaffInvitations.Commands.AcceptInvitation;

using Skill_Loop.Application.Common.Validation; // 👈 استدعاء الـ Extension
using FluentValidation;

public sealed class AcceptInvitationCommandValidator : AbstractValidator<AcceptInvitationCommand>
{
    public AcceptInvitationCommandValidator()
    {
        RuleFor(x => x.InvitationToken)
            .NotEmpty().WithMessage("This field is required.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("This field is required.")
            .MinimumLength(3).WithMessage("Invalid value.");

        // 👇 استخدام السطر الموحد لضمان التطابق التام مع Identity
        RuleFor(x => x.Password)
            .ApplyStandardPasswordRules();

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Invalid value.")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
    }
}