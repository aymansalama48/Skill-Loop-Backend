using Skill_Loop.Application.Common.Errors.Promo;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Application.Features.Promotions.DTOs;

namespace Skill_Loop.Application.Features.Promotions.Queries.ValidatePromoCode;

public record ValidatePromoCodeQuery(string Code) : IRequest<Result<PromoCodeDto>>;

public class ValidatePromoCodeQueryHandler : IRequestHandler<ValidatePromoCodeQuery, Result<PromoCodeDto>>
{
    private readonly IApplicationDbContext _context;

    public ValidatePromoCodeQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PromoCodeDto>> Handle(ValidatePromoCodeQuery request, CancellationToken cancellationToken)
    {
        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        var promo = await _context.PromoCodes
            .FirstOrDefaultAsync(p => p.Code == normalizedCode, cancellationToken);

        if (promo == null)
            return Result<PromoCodeDto>.Failure(PromoErrors.NotFound);

        var checkResult = promo.CheckUsable(DateTime.UtcNow);
        if (checkResult.IsFailure)
            return Result<PromoCodeDto>.Failure(checkResult.Errors.First());

        return Result<PromoCodeDto>.Success(promo.ToDto());
    }
}
