using Skill_Loop.Application.Common.Errors.SupportQuestion;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Features.Support.Queries.GetSupportQuestionDetails;
using Skill_Loop.Application.Features.Support.Share;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Support.Queries.GetSupportQuestionDetails;

public sealed class GetSupportQuestionDetailsQueryHandler : IQueryHandler<GetSupportQuestionDetailsQuery, SupportQuestionDetailsResponse>
{
    private readonly IApplicationDbContext _dbContext;

    public GetSupportQuestionDetailsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<SupportQuestionDetailsResponse>> Handle(GetSupportQuestionDetailsQuery request, CancellationToken cancellationToken)
    {
        var question = await _dbContext.AsNoTracking(_dbContext.SupportQuestions)
            .Where(sq => sq.Id == request.Id)
            .Select(sq => new SupportQuestionDetailsResponse(
                sq.Id,
                sq.Question,
                sq.Answer,
                sq.Category,
                sq.IsPublished,
                sq.UserEmail,
                sq.UserName,
                sq.IsAnswered,
                sq.AnsweredAt,
                sq.EmailSent,
                sq.EmailSentAt,
                sq.CreatedAt,
                sq.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (question is null)
        {
            return Result<SupportQuestionDetailsResponse>.Failure(
                SupportQuestionErrors.NotFound);
        }

        return Result<SupportQuestionDetailsResponse>.Success(question);
    }
}
