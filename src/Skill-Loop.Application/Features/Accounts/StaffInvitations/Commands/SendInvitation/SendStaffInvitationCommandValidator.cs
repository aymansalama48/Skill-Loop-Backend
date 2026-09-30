namespace Skill_Loop.Application.Features.Accounts.StaffInvitations.Commands.SendInvitation;

using Skill_Loop.Domain.Constants;
using FluentValidation;

public sealed class SendStaffInvitationCommandValidator : AbstractValidator<SendStaffInvitationCommand>
{
    public SendStaffInvitationCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("This field is required.")
            .EmailAddress().WithMessage("Invalid email address format.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("This field is required.")
            .WithMessage("Invalid value.");

    }
}