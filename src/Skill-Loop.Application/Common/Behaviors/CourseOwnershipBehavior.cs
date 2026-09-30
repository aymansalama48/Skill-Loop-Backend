using Skill_Loop.Application.Common.Errors.Course;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Application.Features.Courses.Common;
using Skill_Loop.Application.Common.Helpers;
using System.Threading;
using System.Threading.Tasks;

namespace Skill_Loop.Application.Common.Behaviors;

public sealed class CourseOwnershipBehavior<TRequest, TResponse>(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICourseCommand
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (currentUser.IsInRole("Admin") || currentUser.IsInRole("SuperAdmin"))
        {
            return await next();
        }

        var course = await dbContext.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.CourseId, cancellationToken);

        if (course is null)
        {
            var error = CourseErrors.NotFound;
            return ResultFactory.CreateFailure<TResponse>(error);
        }

        if (course.InstructorId != currentUser.UserId)
        {
            var error = CourseErrors.Forbidden;
            return ResultFactory.CreateFailure<TResponse>(error);
        }

        return await next();
    }
}
