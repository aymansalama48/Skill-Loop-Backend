using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Wallets.Shared;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Wallets.Queries.GetMyWallet;

// استعلام بدون كاش لضمان دقة الرصيد المالي اللحظي
[AuthenticatedOnly]
public sealed record GetMyWalletQuery() : IQuery<WalletResponse>;