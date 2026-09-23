using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Courses.Commands.PublishCourse;

public sealed record PublishCourseCommand(Guid CourseId) : ICommand;

public sealed class PublishCourseCommandHandler : ICommandHandler<PublishCourseCommand>
{
    private readonly IApplicationDbContext _context;

    public PublishCourseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(PublishCourseCommand request, CancellationToken cancellationToken)
    {
        var course = await _context.FirstOrDefaultAsync(
            _context.Courses.Where(c => c.Id == request.CourseId),
            cancellationToken);

        if (course is null)
        {
            return Result.Failure(new Error("Course.NotFound", "Course was not found.", ErrorType.NotFound));
        }

        course.Publish();
        _context.Update(course);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
