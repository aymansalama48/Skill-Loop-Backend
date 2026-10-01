using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Sessions;

namespace Skill_Loop.Application.Features.Sessions.Commands.CreateSession;

public sealed class CreateSessionCommandHandler : ICommandHandler<CreateSessionCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser.ICurrentUser _currentUser;

    public CreateSessionCommandHandler(IApplicationDbContext dbContext, Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser.ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(CreateSessionCommand request, CancellationToken cancellationToken)
    {
        bool isAdmin = _currentUser.IsInRole("Admin") || _currentUser.IsInRole("SuperAdmin");
        var ownerId = _currentUser.UserId ?? Guid.Empty;
        var instructorId = isAdmin ? request.InstructorId : ownerId;

        var session = Session.Create(
            instructorId,
            ownerId,
            request.Title,
            request.Description,
            request.ScheduledAtUtc,
            request.DurationMinutes,
            request.CreditsPrice,
            request.LocationType,
            request.LocationDetails,
            request.LiveSessionUrl,
            request.MaxParticipants);

        _dbContext.Add(session);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(session.Id);
    }
}
