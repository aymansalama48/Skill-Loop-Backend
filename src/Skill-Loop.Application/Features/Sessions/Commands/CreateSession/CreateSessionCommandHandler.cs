using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Sessions;

namespace Skill_Loop.Application.Features.Sessions.Commands.CreateSession;

public sealed class CreateSessionCommandHandler : ICommandHandler<CreateSessionCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateSessionCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(CreateSessionCommand request, CancellationToken cancellationToken)
    {
        // بنعتبر المحاضر هو المالك (Owner) للجلسة حالياً
        var session = Session.Create(
            request.InstructorId,
            request.InstructorId,
            request.Title,
            request.Description,
            request.ScheduledAtUtc,
            request.DurationMinutes,
            request.CreditsPrice,
            request.LocationType,
            request.LocationDetails,
            request.MaxParticipants);

        _dbContext.Add(session);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(session.Id);
    }
}
