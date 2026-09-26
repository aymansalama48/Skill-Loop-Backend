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
        // الدالة Create بقت بترجع Result<Session> بدل Session مباشرة ولازم نشيك عليها
        var sessionResult = Session.Create(
            request.InstructorId,
            request.InstructorId,
            request.Title,
            request.PriceInCredits,
            request.DurationInMinutes,
            request.Type);

        if (!sessionResult.IsSuccess)
        {
            return Result<Guid>.Failure(sessionResult.Errors);
        }

        var session = sessionResult.Data;
        _dbContext.Add(session);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(session.Id);
    }
}