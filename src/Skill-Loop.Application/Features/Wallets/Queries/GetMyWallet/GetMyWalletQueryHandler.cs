using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Identity;
using Skill_Loop.Application.Features.Wallets.DTOs;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Wallets;

namespace Skill_Loop.Application.Features.Wallets.Queries.GetMyWallet;

public sealed class GetMyWalletQueryHandler(
    IApplicationDbContext _dbContext,
    ICurrentUser _currentUser) : IQueryHandler<GetMyWalletQuery, WalletDto>
{
    public async Task<Result<WalletDto>> Handle(GetMyWalletQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
        {
            return Result<WalletDto>.Failure(UserErrors.NotFound);
        }

        var userId = _currentUser.UserId.Value;

        var wallet = await _dbContext.UserWallets
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

        // لو المحفظة مش موجودة، بنكريتها أوتوماتيك برصيد صفر (Lazy Creation)
        if (wallet is null)
        {
            wallet = UserWallet.Create(userId, 0);
            _dbContext.Add(wallet);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return Result<WalletDto>.Success(new WalletDto(wallet.Id, wallet.Balance));
    }
}