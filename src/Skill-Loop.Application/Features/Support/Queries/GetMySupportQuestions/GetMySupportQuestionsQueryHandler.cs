using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Support.Queries.GetMySupportQuestions;
using Skill_Loop.Application.Features.Support.Share;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Support.Queries.GetMySupportQuestions;

public sealed class GetMySupportQuestionsQueryHandler : IQueryHandler<GetMySupportQuestionsQuery, PagedResult<MySupportQuestionResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetMySupportQuestionsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PagedResult<MySupportQuestionResponse>>> Handle(GetMySupportQuestionsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.AsNoTracking(_dbContext.SupportQuestions)
            .Where(sq => sq.AskedByUserId == request.UserId);

        var totalCount = await _dbContext.CountAsync(query, cancellationToken);

        var questions = await query
            .OrderByDescending(sq => sq.CreatedAt)
            .Skip(request.Pagination.Skip)
            .Take(request.Pagination.PageSize)
            .Select(sq => new MySupportQuestionResponse(
                sq.Id,
                sq.Question,
                sq.Answer,
                sq.Category,
                sq.IsPublished,
                sq.IsAnswered,
                sq.AnsweredAt,
                sq.CreatedAt))
            .ToListAsync(cancellationToken);

        var paginationMetadata = new PaginationMetadata
        {
            CurrentPage = request.Pagination.PageNumber,
            PageSize = request.Pagination.PageSize,
            TotalCount = totalCount
        };

        var pagedResult = new PagedResult<MySupportQuestionResponse>
        {
            Items = questions,
            Pagination = paginationMetadata
        };

        return Result<PagedResult<MySupportQuestionResponse>>.Success(pagedResult);
    }
}
