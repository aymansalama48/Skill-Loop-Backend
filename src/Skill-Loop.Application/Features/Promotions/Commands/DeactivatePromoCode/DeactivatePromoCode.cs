using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Promotions;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Promotions.Commands.DeactivatePromoCode;

[Permission(Permissions.Finance.PromoCodesManage)]
public sealed record DeactivatePromoCodeCommand(Guid PromoCodeId) : ICommand;

public sealed class DeactivatePromoCodeCommandHandler : ICommandHandler<DeactivatePromoCodeCommand>
{
    private readonly IApplicationDbContext _context;

    public DeactivatePromoCodeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeactivatePromoCodeCommand request, CancellationToken cancellationToken)
    {
        var promo = await _context.FirstOrDefaultAsync(
            _context.PromoCodes.Where(p => p.Id == request.PromoCodeId),
            cancellationToken);

        if (promo is null)
            return Result.Failure(PromoCodeErrors.NotFound);

        promo.Deactivate();
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
