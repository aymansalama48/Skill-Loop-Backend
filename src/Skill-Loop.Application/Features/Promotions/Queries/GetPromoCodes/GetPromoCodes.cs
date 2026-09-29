using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Promotions.DTOs;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Promotions;

namespace Skill_Loop.Application.Features.Promotions.Queries.GetPromoCodes;

/// <summary>قايمة أكواد الخصم للمسؤول المالي، الأحدث فوق.</summary>
public sealed class GetPromoCodesQuery : PaginationParameters, IQuery<PagedResult<PromoCodeDto>>
{
}

public sealed class GetPromoCodesQueryHandler : IQueryHandler<GetPromoCodesQuery, PagedResult<PromoCodeDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPromoCodesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<PromoCodeDto>>> Handle(GetPromoCodesQuery request, CancellationToken cancellationToken)
    {
        IQueryable<PromoCode> baseQuery = _context.AsNoTracking(_context.PromoCodes);

        var totalCount = await _context.CountAsync(baseQuery, cancellationToken);

        var page = await _context.ToListAsync(
            baseQuery.OrderByDescending(p => p.CreatedAt).Skip(request.Skip).Take(request.PageSize),
            cancellationToken);

        return Result<PagedResult<PromoCodeDto>>.Success(new PagedResult<PromoCodeDto>
        {
            Items = page.Select(p => p.ToDto()).ToList(),
            Pagination = new PaginationMetadata
            {
                CurrentPage = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount
            }
        });
    }
}