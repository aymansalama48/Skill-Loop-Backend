namespace Skill_Loop.Application.Features.Accounts.StaffInvitations.Commands.AcceptInvitationWithGoogle;

using FluentValidation;

public sealed class AcceptInvitationWithGoogleCommandValidator : AbstractValidator<AcceptInvitationWithGoogleCommand>
{
    public AcceptInvitationWithGoogleCommandValidator()
    {
        RuleFor(x => x.InvitationToken)
            .NotEmpty().WithMessage("رمز الدعوة مطلوب.");

        RuleFor(x => x.GoogleIdToken)
            .NotEmpty().WithMessage("رمز مصادقة جوجل مطلوب.");
    }
}