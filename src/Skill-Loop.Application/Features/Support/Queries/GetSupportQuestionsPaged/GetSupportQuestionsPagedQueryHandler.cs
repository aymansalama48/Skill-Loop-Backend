using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Support.Queries.GetSupportQuestionsPaged;
using Skill_Loop.Application.Features.Support.Share;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Support.Queries.GetSupportQuestionsPaged;

public sealed class GetSupportQuestionsPagedQueryHandler : IQueryHandler<GetSupportQuestionsPagedQuery, PagedResult<SupportQuestionDetailsResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetSupportQuestionsPagedQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PagedResult<SupportQuestionDetailsResponse>>> Handle(GetSupportQuestionsPagedQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.AsNoTracking(_dbContext.SupportQuestions);

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

        if (request.IsPublished.HasValue)
        {
            query = query.Where(sq => sq.IsPublished == request.IsPublished.Value);
        }

        if (request.IsAnswered.HasValue)
        {
            query = query.Where(sq => sq.IsAnswered == request.IsAnswered.Value);
        }

        var totalCount = await _dbContext.CountAsync(query, cancellationToken);

        var questions = await query
            .OrderByDescending(sq => sq.CreatedAt)
            .Skip(request.Pagination.Skip)
            .Take(request.Pagination.PageSize)
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
            .ToListAsync(cancellationToken);

        var paginationMetadata = new PaginationMetadata
        {
            CurrentPage = request.Pagination.PageNumber,
            PageSize = request.Pagination.PageSize,
            TotalCount = totalCount
        };

        var pagedResult = new PagedResult<SupportQuestionDetailsResponse>
        {
            Items = questions,
            Pagination = paginationMetadata
        };

        return Result<PagedResult<SupportQuestionDetailsResponse>>.Success(pagedResult);
    }
}
