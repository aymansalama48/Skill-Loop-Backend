using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Skill_Loop.Application.Common.Abstractions.External.Media;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Common.Errors;
using Xabe.FFmpeg;

namespace Skill_Loop.Infrastructure.External.Media;

internal sealed class VideoAnalyzer : IVideoAnalyzer
{
    private readonly ILogger<VideoAnalyzer> _logger;
    private static bool _ffmpegInitialized = false;

    public VideoAnalyzer(ILogger<VideoAnalyzer> logger)
    {
        _logger = logger;
    }

    public async Task<Result<VideoMetadata>> AnalyzeVideoAsync(string videoUrlOrPath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_ffmpegInitialized)
            {
                // Attempt to set up ffmpeg path. If it's not installed in PATH, Xabe.FFmpeg uses the current directory or throws.
                _ffmpegInitialized = true;
            }

            // Note: If videoUrlOrPath is a URL, FFmpeg can probe it directly.
            // If it's a local file, it can also probe it directly.
            var mediaInfo = await FFmpeg.GetMediaInfo(videoUrlOrPath, cancellationToken);
            var videoStream = mediaInfo.VideoStreams.FirstOrDefault();

            if (videoStream is null)
            {
                return Result<VideoMetadata>.Failure(new Error("Media.NoVideo", "No video stream found in the provided media.", ErrorType.Failure));
            }

            var duration = mediaInfo.Duration;
            var resolution = $"{videoStream.Width}x{videoStream.Height}";

            return Result<VideoMetadata>.Success(new VideoMetadata(duration, resolution));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to analyze video at {VideoPath}", videoUrlOrPath);
            return Result<VideoMetadata>.Failure(new Error("Media.AnalysisFailed", "Video analysis service is unavailable.", ErrorType.Failure));
        }
    }
}
