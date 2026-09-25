using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Wallets.Shared;

namespace Skill_Loop.Application.Features.Wallets.Queries.GetMyWallet;

// استعلام بدون كاش لضمان دقة الرصيد المالي اللحظي
public sealed record GetMyWalletQuery() : IQuery<WalletResponse>;