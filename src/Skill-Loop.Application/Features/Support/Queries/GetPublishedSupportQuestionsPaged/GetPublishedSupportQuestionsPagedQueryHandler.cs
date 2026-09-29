using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Support.Queries.GetPublishedSupportQuestionsPaged;
using Skill_Loop.Application.Features.Support.Share;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Support.Queries.GetPublishedSupportQuestionsPaged;

public sealed class GetPublishedSupportQuestionsPagedQueryHandler : IQueryHandler<GetPublishedSupportQuestionsPagedQuery, PagedResult<SupportQuestionResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetPublishedSupportQuestionsPagedQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PagedResult<SupportQuestionResponse>>> Handle(GetPublishedSupportQuestionsPagedQuery request, CancellationToken cancellationToken)
    {
        // الـ IsPublished = true ثابتة ومش جاية من الـ Client — الـ Endpoint عام.
        var query = _dbContext.AsNoTracking(_dbContext.SupportQuestions)
            .Where(sq => sq.IsPublished);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.Trim().ToLower();
            query = query.Where(sq =>
                sq.Question.ToLower().Contains(searchTerm) ||
                (sq.Answer != null && sq.Answer.ToLower().Contains(searchTerm)));
        }

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            var category = request.Category.Trim().ToLower();
            query = query.Where(sq => sq.Category.ToLower() == category);
        }

        var totalCount = await _dbContext.CountAsync(query, cancellationToken);

        var questions = await query
            .OrderByDescending(sq => sq.CreatedAt)
            .Skip(request.Pagination.Skip)
            .Take(request.Pagination.PageSize)
            .Select(sq => new SupportQuestionResponse(
                sq.Id,
                sq.Question,
                sq.Answer,
                sq.Category,
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

        var pagedResult = new PagedResult<SupportQuestionResponse>
        {
            Items = questions,
            Pagination = paginationMetadata
        };

        return Result<PagedResult<SupportQuestionResponse>>.Success(pagedResult);
    }
}
