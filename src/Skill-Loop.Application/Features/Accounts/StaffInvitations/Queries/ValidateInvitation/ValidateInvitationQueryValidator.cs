namespace Skill_Loop.Application.Features.Accounts.StaffInvitations.Queries.ValidateInvitation;

using FluentValidation;

public sealed class ValidateInvitationQueryValidator : AbstractValidator<ValidateInvitationQuery>
{
    public ValidateInvitationQueryValidator()
    {
        // افترضت أن اسم الخاصية هو Token (عدلها لو كان اسمها InvitationToken)
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("رمز الدعوة (Token) مطلوب ولا يمكن أن يكون فارغاً.");
    }
}