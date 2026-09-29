using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Support.Commands.SetSupportQuestionPublication;

public sealed class SetSupportQuestionPublicationCommandHandler : ICommandHandler<SetSupportQuestionPublicationCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public SetSupportQuestionPublicationCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(SetSupportQuestionPublicationCommand request, CancellationToken cancellationToken)
    {
        var question = await _dbContext.SupportQuestions
            .FirstOrDefaultAsync(sq => sq.Id == request.Id, cancellationToken);

        if (question is null)
        {
            return Result.Failure(
                new Error("SupportQuestion.NotFound", "Support question not found.", ErrorType.NotFound));
        }

        var result = request.IsPublished ? question.Publish() : question.Unpublish();

        if (result.IsFailure)
            return Result.Failure(result.Errors);

        _dbContext.Update(question);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
