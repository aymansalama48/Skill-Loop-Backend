namespace Skill_Loop.Application.Features.Accounts.StaffInvitations.Commands.AcceptInvitationWithGoogle;

using FluentValidation;

public sealed class AcceptInvitationWithGoogleCommandValidator : AbstractValidator<AcceptInvitationWithGoogleCommand>
{
    public AcceptInvitationWithGoogleCommandValidator()
    {
        RuleFor(x => x.InvitationToken)
            .NotEmpty().WithMessage("This field is required.");

        RuleFor(x => x.GoogleIdToken)
            .NotEmpty().WithMessage("This field is required.");
    }
}