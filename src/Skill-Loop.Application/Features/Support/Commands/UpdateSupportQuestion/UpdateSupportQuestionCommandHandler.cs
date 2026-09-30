using Skill_Loop.Application.Common.Errors.SupportQuestion;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Support.Commands.UpdateSupportQuestion;

public sealed class UpdateSupportQuestionCommandHandler : ICommandHandler<UpdateSupportQuestionCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateSupportQuestionCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(UpdateSupportQuestionCommand request, CancellationToken cancellationToken)
    {
        var question = await _dbContext.SupportQuestions
            .FirstOrDefaultAsync(sq => sq.Id == request.Id, cancellationToken);

        if (question is null)
        {
            return Result.Failure(
                SupportQuestionErrors.NotFound);
        }

        var updateResult = question.Update(request.Question, request.Answer, request.Category, request.IsPublished);
        if (updateResult.IsFailure)
            return Result.Failure(updateResult.Errors);

        _dbContext.Update(question);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
