using Skill_Loop.Domain.Common.Entities;
using Skill_Loop.Domain.Entities.Courses.ValueObjects;

namespace Skill_Loop.Domain.Entities.Courses;

public sealed class Lesson : BaseEntity
{
    public Guid SectionId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public VideoResource Video { get; private set; } = null!;
    public int OrderIndex { get; private set; }
    public bool IsPreviewable { get; private set; }

    private readonly List<PdfAttachment> _resources = [];
    public IReadOnlyCollection<PdfAttachment> Resources => _resources.AsReadOnly();

    private Lesson() { }

    internal static Lesson Create(Guid sectionId, string title, VideoResource video, int orderIndex, bool isPreviewable) =>
        new()
        {
            Id = Guid.CreateVersion7(),
            SectionId = sectionId,
            Title = title.Trim(),
            Video = video,
            OrderIndex = orderIndex,
            IsPreviewable = isPreviewable
        };

    public void AddResource(PdfAttachment attachment) => _resources.Add(attachment);
}
