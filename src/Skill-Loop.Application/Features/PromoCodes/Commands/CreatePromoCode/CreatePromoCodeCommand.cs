using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Promotions;
using Skill_Loop.Domain.Enums;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.PromoCodes.Commands.CreatePromoCode;

[Permission(Permissions.Finance.PromoCodesManage)]
public record CreatePromoCodeCommand(
    string Code,
    DiscountType DiscountType,
    int DiscountValue,
    int? MaxRedemptions,
    DateTime? ExpiresAt) : IRequest<Result<Guid>>;

public class CreatePromoCodeCommandValidator : AbstractValidator<CreatePromoCodeCommand>
{
    public CreatePromoCodeCommandValidator()
    {
        RuleFor(v => v.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters.");

        RuleFor(v => v.DiscountType)
            .IsInEnum().WithMessage("Invalid discount type.");

        RuleFor(v => v.DiscountValue)
            .GreaterThan(0).WithMessage("Discount value must be greater than 0.");
            
        RuleFor(v => v.DiscountValue)
            .LessThanOrEqualTo(100).When(v => v.DiscountType == DiscountType.Percentage)
            .WithMessage("Percentage discount must not exceed 100.");

        RuleFor(v => v.MaxRedemptions)
            .GreaterThan(0).When(v => v.MaxRedemptions.HasValue)
            .WithMessage("Max redemptions must be greater than 0.");
    }
}

public class CreatePromoCodeCommandHandler : IRequestHandler<CreatePromoCodeCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreatePromoCodeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreatePromoCodeCommand request, CancellationToken cancellationToken)
    {
        var existingPromo = await _context.PromoCodes
            .FirstOrDefaultAsync(p => p.Code == request.Code, cancellationToken);

        if (existingPromo != null)
            return Result<Guid>.Failure($"Promo code '{request.Code}' already exists.");

        var promoCodeResult = PromoCode.Create(
            request.Code,
            request.DiscountType,
            request.DiscountValue,
            request.MaxRedemptions,
            request.ExpiresAt);
            
        if (promoCodeResult.IsFailure)
            return Result<Guid>.Failure(promoCodeResult.Errors);

        _context.Add(promoCodeResult.Data!);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(promoCodeResult.Data!.Id);
    }
}
