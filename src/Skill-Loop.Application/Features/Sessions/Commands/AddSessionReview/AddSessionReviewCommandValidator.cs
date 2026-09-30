using FluentValidation;

namespace Skill_Loop.Application.Features.Sessions.Commands.AddSessionReview;

public sealed class AddSessionReviewCommandValidator : AbstractValidator<AddSessionReviewCommand>
{
    public AddSessionReviewCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.SessionId).NotEmpty();
        RuleFor(x => x.Stars).InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.");
        RuleFor(x => x.Comment).MaximumLength(2000).When(x => !string.IsNullOrEmpty(x.Comment));
    }
}
