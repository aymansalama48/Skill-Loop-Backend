using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Courses;


namespace Skill_Loop.Application.Features.Courses.Commands.CreateCourse;

public sealed class CreateCourseCommandHandler : ICommandHandler<CreateCourseCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateCourseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
    {
        var categoryExists = await _context.AnyAsync(
            _context.Categories.Where(c => c.Id == request.CategoryId),
            cancellationToken);

        if (!categoryExists)
        {
            return Result<Guid>.Failure(new Error("Category.NotFound", "The specified category was not found.", ErrorType.NotFound));
        }

        var courseResult = Course.Create(
            request.Title,
            request.Description,
            request.ThumbnailUrl,
            request.Credits,
            request.Level,
            request.InstructorId,
            request.InstructorName,
            request.CategoryId);

        if (courseResult.IsFailure)
        {
            return Result<Guid>.Failure(courseResult.Errors.First());
        }

        var course = courseResult.Data!;
        _context.Add(course);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(course.Id);
    }
}