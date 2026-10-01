using FluentValidation;
using Skill_Loop.Application.Common.Abstractions.Messaging;

namespace Skill_Loop.Application.Features.Sessions.Commands.UpdateSessionReview;

public sealed record UpdateSessionReviewCommand(
    Guid SessionId,
    Guid UserId,
    int Stars,
    string? Comment) : ICommand;

public sealed class UpdateSessionReviewCommandValidator : AbstractValidator<UpdateSessionReviewCommand>
{
    public UpdateSessionReviewCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Stars).InclusiveBetween(1, 5);
        RuleFor(x => x.Comment).MaximumLength(1000);
    }
}
