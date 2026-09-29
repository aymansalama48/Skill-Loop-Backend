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
        var result = string.IsNullOrWhiteSpace(request.Answer)
            ? SupportQuestion.Create(request.Question, request.Category)
            : SupportQuestion.CreatePublished(request.Question, request.Answer!, request.Category);

        if (result.IsFailure)
            return Result<Guid>.Failure(result.Errors);

        var question = result.Data!;

        _dbContext.Add(question);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(question.Id);
    }
}