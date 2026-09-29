namespace Skill_Loop.Application.Features.Wallets.Shared;

public sealed record WalletResponse(
    Guid WalletId,
    int Balance
);