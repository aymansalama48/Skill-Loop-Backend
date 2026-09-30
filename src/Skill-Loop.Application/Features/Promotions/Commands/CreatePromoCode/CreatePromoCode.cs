using FluentValidation;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Promotions;
using Skill_Loop.Application.Features.Promotions.DTOs;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Promotions;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Application.Features.Promotions.Commands.CreatePromoCode;

/// <param name="DiscountType">"Percentage" أو "FixedAmount".</param>
/// <param name="DiscountValue">نسبة (1-100) أو مبلغ بأصغر وحدة عملة.</param>
public sealed record CreatePromoCodeCommand(
    string Code,
    string DiscountType,
    int DiscountValue,
    int? MaxRedemptions,
    DateTime? ExpiresAt) : ICommand<PromoCodeDto>;

public sealed class CreatePromoCodeCommandValidator : AbstractValidator<CreatePromoCodeCommand>
{
    public CreatePromoCodeCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().WithMessage("This field is required.");
        RuleFor(x => x.DiscountType)
            .Must(t => Enum.TryParse<DiscountType>(t, ignoreCase: true, out _))
            .WithMessage("Invalid value.");
        RuleFor(x => x.DiscountValue).GreaterThan(0).WithMessage("Invalid value.");
    }
}

public sealed class CreatePromoCodeCommandHandler : ICommandHandler<CreatePromoCodeCommand, PromoCodeDto>
{
    private readonly IApplicationDbContext _context;

    public CreatePromoCodeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PromoCodeDto>> Handle(CreatePromoCodeCommand request, CancellationToken cancellationToken)
    {
        var normalized = PromoCode.NormalizeCode(request.Code);

        var exists = await _context.AnyAsync(
            _context.PromoCodes.Where(p => p.Code == normalized),
            cancellationToken);

        if (exists)
            return Result<PromoCodeDto>.Failure(PromoCodeErrors.DuplicateCode);

        var type = Enum.Parse<DiscountType>(request.DiscountType, ignoreCase: true);

        var createResult = PromoCode.Create(request.Code, type, request.DiscountValue, request.MaxRedemptions, request.ExpiresAt);
        if (createResult.IsFailure)
            return Result<PromoCodeDto>.Failure(createResult.Errors);

        var promo = createResult.Data!;
        _context.Add(promo);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<PromoCodeDto>.Success(promo.ToDto());
    }
}
