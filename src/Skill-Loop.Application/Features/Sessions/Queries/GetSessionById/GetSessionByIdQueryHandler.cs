using Skill_Loop.Application.Common.Errors.Session;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Features.Sessions.Queries.Share;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Application.Features.Sessions.Queries.GetSessionById;

public sealed class GetSessionByIdQueryHandler : IQueryHandler<GetSessionByIdQuery, SessionResponse>
{
    private readonly IApplicationDbContext _dbContext;

    public GetSessionByIdQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<SessionResponse>> Handle(GetSessionByIdQuery request, CancellationToken cancellationToken)
    {
        var session = await _dbContext.Sessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (session is null)
        {
            return Result<SessionResponse>.Failure(SessionErrors.NotFound);
        }

        // عدد الحجوزات النشطة عشان نحسب المقاعد المتاحة
        var bookedParticipants = await SessionAvailabilityHelper.CountActiveBookingsAsync(
            _dbContext, session.Id, cancellationToken);

        var response = SessionAvailabilityHelper.BuildResponse(session, bookedParticipants, DateTime.UtcNow);

        return Result<SessionResponse>.Success(response);
    }
}
