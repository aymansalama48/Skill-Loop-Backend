using Skill_Loop.Application.Common.Errors.Session;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Enums;

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
            return Result.Failure(SessionErrors.NotFound);
        }

        // 1. لو فيه حجوزات نشطة، ممنوع نغير الموعد أو السعر (الحجوزات محسوبة بالسعر القديم)
        var activeBookings = await _dbContext.Bookings
            .CountAsync(b =>
                b.SessionId == session.Id &&
                (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.InProgress),
                cancellationToken);

        if (activeBookings > 0 && (request.ScheduledAtUtc.HasValue || request.CreditsPrice.HasValue))
        {
            return Result.Failure(SessionErrors.HasActiveBookings);
        }

        // 2. التحديث (Description بتبقى null معناها "سيبها زي ما هي")
        session.UpdateDetails(request.Title, request.Description);

        session.UpdateSchedule(
            request.ScheduledAtUtc,
            request.DurationMinutes,
            request.CreditsPrice,
            request.LocationType,
            request.LocationDetails,
            request.MaxParticipants);

        _dbContext.Update(session);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
