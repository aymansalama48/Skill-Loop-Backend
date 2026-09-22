using Skill_Loop.Domain.Common.Entities;

namespace Skill_Loop.Domain.Entities.Courses;

public sealed class Section : BaseEntity
{
    public Guid CourseId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public int OrderIndex { get; private set; }

    private readonly List<Lesson> _lessons = [];
    public IReadOnlyCollection<Lesson> Lessons => _lessons.AsReadOnly();

    private Section() { }

    internal static Section Create(Guid courseId, string title, int orderIndex) =>
        new()
        {
            Id = Guid.CreateVersion7(),
            CourseId = courseId,
            Title = title,
            OrderIndex = orderIndex
        };

    internal void AddLesson(Lesson lesson) => _lessons.Add(lesson);
}
