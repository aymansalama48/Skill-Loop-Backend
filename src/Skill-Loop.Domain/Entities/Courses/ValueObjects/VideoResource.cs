using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Domain.Entities.Courses.ValueObjects;

public sealed record VideoResource
{
    public string VideoUrl { get; private init; } = string.Empty;
    public TimeSpan Duration { get; private init; }
    public string? StreamingResolution { get; private init; }
    public string? ExternalProviderId { get; private init; }

    private VideoResource() { }

    private VideoResource(string videoUrl, TimeSpan duration, string? streamingResolution, string? externalProviderId)
    {
        VideoUrl = videoUrl;
        Duration = duration;
        StreamingResolution = streamingResolution;
        ExternalProviderId = externalProviderId;
    }

    public static Result<VideoResource> Create(
        string videoUrl,
        TimeSpan duration,
        string? streamingResolution = "1080p",
        string? externalProviderId = null)
    {
        if (string.IsNullOrWhiteSpace(videoUrl))
        {
            return Result<VideoResource>.Failure(new Error("VideoResource.EmptyUrl", "Video URL is required.", ErrorType.Validation));
        }

        if (duration <= TimeSpan.Zero)
        {
            return Result<VideoResource>.Failure(new Error("VideoResource.InvalidDuration", "Duration must be greater than zero.", ErrorType.Validation));
        }

        return Result<VideoResource>.Success(new VideoResource(videoUrl.Trim(), duration, streamingResolution, externalProviderId));
    }
}
