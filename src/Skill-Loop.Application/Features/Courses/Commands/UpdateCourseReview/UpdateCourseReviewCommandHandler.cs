using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Errors.Auth;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Errors.Course;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Courses.Commands.UpdateCourseReview;

internal sealed class UpdateCourseReviewCommandHandler(IApplicationDbContext _dbContext, ICurrentUser _currentUser) : ICommandHandler<UpdateCourseReviewCommand>
{
    public async Task<Result> Handle(UpdateCourseReviewCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId != request.UserId)
        {
            return Result.Failure(AuthErrors.Forbidden);
        }
        var course = await _dbContext.Courses
            .Include(c => c.Reviews)
            .FirstOrDefaultAsync(c => c.Id == request.CourseId, cancellationToken);

        if (course is null)
        {
            return Result.Failure(CourseErrors.NotFound);
        }

        var result = course.UpdateReview(request.UserId, request.Stars, request.Comment);

        if (result.IsFailure)
        {
            return result;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
