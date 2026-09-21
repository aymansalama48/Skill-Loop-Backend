namespace Skill_Loop.Application.Features.Accounts.StaffInvitations.Commands.SendInvitation;

using Skill_Loop.Domain.Constants;
using FluentValidation;

public sealed class SendStaffInvitationCommandValidator : AbstractValidator<SendStaffInvitationCommand>
{
    public SendStaffInvitationCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("البريد الإلكتروني مطلوب.")
            .EmailAddress().WithMessage("صيغة البريد الإلكتروني غير صحيحة.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("الدور الوظيفي مطلوب.")
            .WithMessage("الدور الوظيفي غير صالح.");

    }
}