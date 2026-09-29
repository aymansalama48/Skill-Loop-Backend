using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Instructors;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Instructors.Commands.UpdateInstructorReview;

public sealed class UpdateInstructorReviewCommandHandler(
    IApplicationDbContext _dbContext) : ICommandHandler<UpdateInstructorReviewCommand, bool>
{
    public async Task<Result<bool>> Handle(UpdateInstructorReviewCommand request, CancellationToken cancellationToken)
    {
        var profile = await _dbContext.InstructorProfiles
            .Include(p => p.Reviews)
            .FirstOrDefaultAsync(p => p.Id == request.InstructorProfileId, cancellationToken);

        if (profile is null)
        {
            return Result<bool>.Failure(InstructorProfileErrors.NotFound);
        }

        var updateResult = profile.UpdateReview(request.ReviewId, request.LearnerUserId, request.Rating, request.Comment);

        if (!updateResult.IsSuccess)
        {
            return Result<bool>.Failure(updateResult.Errors);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}