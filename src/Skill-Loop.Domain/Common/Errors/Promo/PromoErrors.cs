using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Domain.Common.Errors.Promo;

public static class PromoErrors
{
    public static readonly Error InvalidCode = new Error(
        "Promo.InvalidCode",
        "Promo invalid code.",
        ErrorType.Validation);

    public static readonly Error InvalidPercentage = new Error(
        "Promo.InvalidPercentage",
        "Promo invalid percentage.",
        ErrorType.Validation);

    public static readonly Error InvalidAmount = new Error(
        "Promo.InvalidAmount",
        "Promo invalid amount.",
        ErrorType.Validation);

    public static readonly Error InvalidMaxRedemptions = new Error(
        "Promo.InvalidMaxRedemptions",
        "Promo invalid max redemptions.",
        ErrorType.Validation);

    public static readonly Error InvalidExpiry = new Error(
        "Promo.InvalidExpiry",
        "Promo invalid expiry.",
        ErrorType.Validation);

    public static readonly Error Inactive = new Error(
        "Promo.Inactive",
        "Promo inactive.",
        ErrorType.Validation);

    public static readonly Error Expired = new Error(
        "Promo.Expired",
        "You do not have permission to modify this promo.",
        ErrorType.Validation);

    public static readonly Error Exhausted = new Error(
        "Promo.Exhausted",
        "Promo exhausted.",
        ErrorType.Validation);

}
