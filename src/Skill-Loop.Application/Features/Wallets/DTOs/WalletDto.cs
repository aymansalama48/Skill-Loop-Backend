namespace Skill_Loop.Application.Features.Wallets.DTOs;

public sealed record WalletDto(
    Guid WalletId,
    int Balance
);