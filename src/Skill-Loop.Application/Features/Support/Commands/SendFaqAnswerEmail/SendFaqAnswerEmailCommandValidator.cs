using FluentValidation;
using Skill_Loop.Application.Features.Support.Commands.SendFaqAnswerEmail;

namespace Skill_Loop.Application.Features.Support.Commands.SendFaqAnswerEmail;

public sealed class SendFaqAnswerEmailCommandValidator : AbstractValidator<SendFaqAnswerEmailCommand>
{
    public SendFaqAnswerEmailCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Question id is required.");
    }
}
