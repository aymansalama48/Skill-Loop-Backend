using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Errors.Wallet;

public static class WalletErrors
{
    public static readonly Error NotFound = new Error(
        "Wallet.NotFound",
        "Wallet was not found.",
        ErrorType.NotFound);

}
