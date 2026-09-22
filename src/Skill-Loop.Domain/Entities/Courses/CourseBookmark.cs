using Skill_Loop.Domain.Common.Entities;

namespace Skill_Loop.Domain.Entities.Courses;

public sealed class CourseBookmark : SoftDeleteEntity
{
    public Guid UserId { get; private set; }
    public Guid CourseId { get; private set; }
    public Course? Course { get; private set; }

    private CourseBookmark() { }

    public static CourseBookmark Create(Guid userId, Guid courseId) =>
        new()
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            CourseId = courseId
        };
}
