using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Courses.Commands.AddCourseReview;

public sealed class AddCourseReviewCommandHandler : ICommandHandler<AddCourseReviewCommand>
{
    private readonly IApplicationDbContext _context;

    public AddCourseReviewCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(AddCourseReviewCommand request, CancellationToken cancellationToken)
    {
        var course = await _context.FirstOrDefaultAsync(
            _context.Courses.Where(c => c.Id == request.CourseId),
            cancellationToken);

        if (course is null)
        {
            return Result.Failure(new Error("Course.NotFound", "Course was not found.", ErrorType.NotFound));
        }

        var reviewResult = course.AddReview(request.UserId, request.Stars, request.Comment);
        if (reviewResult.IsFailure)
        {
            return Result.Failure(reviewResult.Errors.First());
        }

        _context.Update(course);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}