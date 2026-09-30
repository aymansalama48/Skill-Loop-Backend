namespace Skill_Loop.Application.Features.Accounts.StaffInvitations.Queries.ValidateInvitation;

using FluentValidation;

public sealed class ValidateInvitationQueryValidator : AbstractValidator<ValidateInvitationQuery>
{
    public ValidateInvitationQueryValidator()
    {
        // افترضت أن اسم الخاصية هو Token (عدلها لو كان اسمها InvitationToken)
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("This field is required.");
    }
}