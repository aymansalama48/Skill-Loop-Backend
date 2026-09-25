using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Wallets.DTOs;

namespace Skill_Loop.Application.Features.Wallets.Queries.GetMyWalletTransactionsPaged;

public sealed record GetMyWalletTransactionsPagedQuery(
    int PageNumber,
    int PageSize
) : IQuery<PagedResult<WalletTransactionDto>>;