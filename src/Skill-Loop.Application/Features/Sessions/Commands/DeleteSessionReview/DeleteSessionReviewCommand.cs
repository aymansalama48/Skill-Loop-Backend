using FluentValidation;
using Skill_Loop.Application.Common.Abstractions.Messaging;

namespace Skill_Loop.Application.Features.Sessions.Commands.DeleteSessionReview;

public sealed record DeleteSessionReviewCommand(Guid SessionId, Guid UserId) : ICommand;

public sealed class DeleteSessionReviewCommandValidator : AbstractValidator<DeleteSessionReviewCommand>
{
    public DeleteSessionReviewCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}
