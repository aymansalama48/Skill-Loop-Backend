using Skill_Loop.Domain.Common.Entities;


namespace Skill_Loop.Domain.Entities.Courses;

public sealed class Lesson : BaseEntity
{
    public Guid SectionId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string VideoUrl { get; private set; } = string.Empty;
    public TimeSpan Duration { get; private set; }
    public string? StreamingResolution { get; private set; }
    public string? ExternalProviderId { get; private set; }
    public int OrderIndex { get; private set; }
    public bool IsPreviewable { get; private set; }

    private readonly List<LessonMaterial> _resources = [];
    public IReadOnlyCollection<LessonMaterial> Resources => _resources.AsReadOnly();

    private Lesson() { }

    internal static Lesson Create(Guid sectionId, string title, int orderIndex, bool isPreviewable) =>
        new()
        {
            Id = Guid.CreateVersion7(),
            SectionId = sectionId,
            Title = title.Trim(),
            VideoUrl = string.Empty,
            Duration = TimeSpan.Zero,
            StreamingResolution = null,
            ExternalProviderId = null,
            OrderIndex = orderIndex,
            IsPreviewable = isPreviewable
        };

    public void AddResource(LessonMaterial attachment) => _resources.Add(attachment);

    internal void UpdateDetails(string title, int orderIndex, bool isPreviewable)
    {
        Title = title.Trim();
        OrderIndex = orderIndex;
        IsPreviewable = isPreviewable;
    }

    internal void UpdateVideo(string videoUrl, TimeSpan duration, string? streamingResolution, string? externalProviderId)
    {
        VideoUrl = videoUrl;
        Duration = duration;
        StreamingResolution = streamingResolution;
        ExternalProviderId = externalProviderId;
    }

    internal void UpdateOrder(int orderIndex)
    {
        OrderIndex = orderIndex;
    }
}
