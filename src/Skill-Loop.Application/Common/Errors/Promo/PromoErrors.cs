using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Errors.Promo;

public static class PromoErrors
{
    public static readonly Error NotFound = new Error(
        "Promo.NotFound",
        "Promo was not found.",
        ErrorType.NotFound);

}
