using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Identity;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Wallets.Shared;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Wallets;

namespace Skill_Loop.Application.Features.Wallets.Queries.GetMyWalletTransactionsPaged;

public sealed class GetMyWalletTransactionsPagedQueryHandler(
    IApplicationDbContext _dbContext,
    ICurrentUser _currentUser) : IQueryHandler<GetMyWalletTransactionsPagedQuery, PagedResult<WalletTransactionResponse>>
{
    public async Task<Result<PagedResult<WalletTransactionResponse>>> Handle(GetMyWalletTransactionsPagedQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
        {
            return Result<PagedResult<WalletTransactionResponse>>.Failure(UserErrors.NotFound);
        }

        var userId = _currentUser.UserId.Value;

        // جلب المحفظة لمعرفة הـ WalletId
        var walletId = await _dbContext.UserWallets
            .AsNoTracking()
            .Where(w => w.UserId == userId)
            .Select(w => w.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (walletId == Guid.Empty)
        {
            // لو مفيش محفظة، نرجع لستة فاضية
            return Result<PagedResult<WalletTransactionResponse>>.Success(new PagedResult<WalletTransactionResponse>
            {
                Items = [],
                Pagination = new PaginationMetadata { TotalCount = 0, PageSize = request.PageSize, CurrentPage = request.PageNumber }
            });
        }

        var query = _dbContext.WalletTransactions // بافتراض إن عندك DbSet<WalletTransaction> في الـ DbContext
            .AsNoTracking()
            .Where(t => t.WalletId == walletId);

        var totalCount = await query.CountAsync(cancellationToken);

        var transactions = await query
            .OrderByDescending(t => t.OccurredAt) // أحدث الحركات الأول
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new WalletTransactionResponse(
                t.Id,
                t.Amount,
                t.Type.ToString(),
                t.Description,
                t.OccurredAt))
            .ToListAsync(cancellationToken);

        var pagedResult = new PagedResult<WalletTransactionResponse>
        {
            Items = transactions,
            Pagination = new PaginationMetadata
            {
                TotalCount = totalCount,
                PageSize = request.PageSize,
                CurrentPage = request.PageNumber
            }
        };

        return Result<PagedResult<WalletTransactionResponse>>.Success(pagedResult);
    }
}