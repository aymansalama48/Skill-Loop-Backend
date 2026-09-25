using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Instructors;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Instructors.Commands.RemoveInstructorReview;

public sealed class RemoveInstructorReviewCommandHandler(
    IApplicationDbContext _dbContext) : ICommandHandler<RemoveInstructorReviewCommand, bool>
{
    public async Task<Result<bool>> Handle(RemoveInstructorReviewCommand request, CancellationToken cancellationToken)
    {
        var profile = await _dbContext.InstructorProfiles
            .Include(p => p.Reviews)
            .FirstOrDefaultAsync(p => p.Id == request.InstructorProfileId, cancellationToken);

        if (profile is null)
        {
            return Result<bool>.Failure(InstructorProfileErrors.NotFound);
        }

        var removeResult = profile.RemoveReview(request.ReviewId, request.LearnerUserId);

        if (!removeResult.IsSuccess)
        {
            return Result<bool>.Failure(removeResult.Errors);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}