using Skill_Loop.Domain.Common.Entities;

namespace Skill_Loop.Domain.Entities.Courses;

public sealed class CourseReview : AuditableEntity
{
    public Guid CourseId { get; private set; }
    public Guid UserId { get; private set; }
    public int Stars { get; private set; }
    public string? Comment { get; private set; }

    private CourseReview() { }

    internal static CourseReview Create(Guid courseId, Guid userId, int stars, string? comment) =>
        new()
        {
            Id = Guid.CreateVersion7(),
            CourseId = courseId,
            UserId = userId,
            Stars = stars,
            Comment = comment?.Trim()
        };
}
