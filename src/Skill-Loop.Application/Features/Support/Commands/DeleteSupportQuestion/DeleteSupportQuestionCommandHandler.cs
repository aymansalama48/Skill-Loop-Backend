using Skill_Loop.Application.Common.Errors.SupportQuestion;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Support.Commands.DeleteSupportQuestion;

public sealed class DeleteSupportQuestionCommandHandler : ICommandHandler<DeleteSupportQuestionCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public DeleteSupportQuestionCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(DeleteSupportQuestionCommand request, CancellationToken cancellationToken)
    {
        var question = await _dbContext.SupportQuestions
            .FirstOrDefaultAsync(sq => sq.Id == request.Id, cancellationToken);

        if (question is null)
        {
            return Result.Failure(
                SupportQuestionErrors.NotFound);
        }

        _dbContext.Remove(question);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
