using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Sessions.Commands.UpdateSession;

public sealed class UpdateSessionCommandHandler : ICommandHandler<UpdateSessionCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateSessionCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(UpdateSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _dbContext.Sessions
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (session is null)
        {
            return Result.Failure(new Error("Session.NotFound", "الجلسة غير موجودة.", ErrorType.NotFound));
        }

        // استخدام دالة UpdateDetails بالخصائص الجديدة
        session.UpdateDetails(
            request.Title,
            request.PriceInCredits,
            request.DurationInMinutes,
            request.Type);

        _dbContext.Update(session);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}