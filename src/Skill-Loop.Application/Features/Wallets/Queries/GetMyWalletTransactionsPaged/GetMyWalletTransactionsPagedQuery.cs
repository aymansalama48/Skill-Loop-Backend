using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Wallets.Shared;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Wallets.Queries.GetMyWalletTransactionsPaged;

[AuthenticatedOnly]
public sealed record GetMyWalletTransactionsPagedQuery(
    int PageNumber,
    int PageSize
) : IQuery<PagedResult<WalletTransactionResponse>>;