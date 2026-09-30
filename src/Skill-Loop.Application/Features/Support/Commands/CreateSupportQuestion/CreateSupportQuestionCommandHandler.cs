using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Features.Support.Commands.CreateSupportQuestion;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Support;

namespace Skill_Loop.Application.Features.Support.Commands.CreateSupportQuestion;

public sealed class CreateSupportQuestionCommandHandler : ICommandHandler<CreateSupportQuestionCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateSupportQuestionCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(CreateSupportQuestionCommand request, CancellationToken cancellationToken)
    {
        // Only publish when the caller explicitly asked for it AND an answer exists.
        // A supplied answer with IsPublished=false stays an internal draft.
        var hasAnswer = !string.IsNullOrWhiteSpace(request.Answer);

        var result = request.IsPublished && hasAnswer
            ? SupportQuestion.CreatePublished(request.Question, request.Answer!, request.Category)
            : SupportQuestion.Create(request.Question, request.Category);

        if (result.IsFailure)
            return Result<Guid>.Failure(result.Errors);

        var question = result.Data!;

        if (hasAnswer)
        {
            var answerResult = question.SetAnswer(request.Answer!);
            if (answerResult.IsFailure)
                return Result<Guid>.Failure(answerResult.Errors);
        }

        _dbContext.Add(question);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(question.Id);
    }
}