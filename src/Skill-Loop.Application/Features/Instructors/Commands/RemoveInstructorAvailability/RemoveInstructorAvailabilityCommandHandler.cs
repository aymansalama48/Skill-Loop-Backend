using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Instructors;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Instructors.Commands.RemoveInstructorAvailability;

public sealed class RemoveInstructorAvailabilityCommandHandler(
    IApplicationDbContext _dbContext) : ICommandHandler<RemoveInstructorAvailabilityCommand, bool>
{
    public async Task<Result<bool>> Handle(RemoveInstructorAvailabilityCommand request, CancellationToken cancellationToken)
    {
        var profile = await _dbContext.InstructorProfiles
            .Include(p => p.Availabilities)
            .FirstOrDefaultAsync(p => p.Id == request.InstructorProfileId, cancellationToken);

        if (profile is null)
        {
            return Result<bool>.Failure(InstructorProfileErrors.NotFound);
        }

        var removeResult = profile.RemoveAvailability(request.AvailabilityId);

        if (!removeResult.IsSuccess)
        {
            return Result<bool>.Failure(removeResult.Errors);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}