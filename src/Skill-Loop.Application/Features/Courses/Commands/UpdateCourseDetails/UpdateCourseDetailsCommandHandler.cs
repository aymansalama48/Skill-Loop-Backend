using Skill_Loop.Application.Common.Errors.Course;
using Skill_Loop.Application.Common.Errors.Category;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Skill_Loop.Application.Features.Courses.Commands.UpdateCourseDetails;

public sealed class UpdateCourseDetailsCommandHandler : ICommandHandler<UpdateCourseDetailsCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateCourseDetailsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateCourseDetailsCommand request, CancellationToken cancellationToken)
    {
        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == request.CategoryId, cancellationToken);
            
        if (!categoryExists)
        {
            return Result.Failure(CategoryErrors.NotFound);
        }

        var course = await _context.FirstOrDefaultAsync(
            _context.Courses.Where(c => c.Id == request.CourseId),
            cancellationToken);

        if (course is null)
        {
            return Result.Failure(CourseErrors.NotFound);
        }

        var updateResult = course.UpdateDetails(
            request.Title,
            request.Description,
            course.ThumbnailUrl, // Keep the existing thumbnail, it is updated separately
            request.Credits,
            request.Level,
            request.CategoryId);

        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
