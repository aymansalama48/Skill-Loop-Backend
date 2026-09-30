using Skill_Loop.Application.Common.Errors.SupportQuestion;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Features.Support.Queries.GetSupportQuestionById;
using Skill_Loop.Application.Features.Support.Share;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Support.Queries.GetSupportQuestionById;

public sealed class GetSupportQuestionByIdQueryHandler : IQueryHandler<GetSupportQuestionByIdQuery, SupportQuestionResponse>
{
    private readonly IApplicationDbContext _dbContext;

    public GetSupportQuestionByIdQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<SupportQuestionResponse>> Handle(GetSupportQuestionByIdQuery request, CancellationToken cancellationToken)
    {
        // الـ IsPublished شرط ثابت — الـ Endpoint عام ومينفعش حد يقرا سؤال لسه متجاوبش عليه.
        var question = await _dbContext.AsNoTracking(_dbContext.SupportQuestions)
            .Where(sq => sq.Id == request.Id && sq.IsPublished)
            .Select(sq => new SupportQuestionResponse(
                sq.Id,
                sq.Question,
                sq.Answer,
                sq.Category,
                sq.IsAnswered,
                sq.AnsweredAt,
                sq.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (question is null)
        {
            return Result<SupportQuestionResponse>.Failure(
                SupportQuestionErrors.NotFound);
        }

        return Result<SupportQuestionResponse>.Success(question);
    }
}
