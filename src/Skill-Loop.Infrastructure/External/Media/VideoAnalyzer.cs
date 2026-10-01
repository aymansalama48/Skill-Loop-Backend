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
using Xabe.FFmpeg.Downloader; // تأكد من وجود ده

namespace Skill_Loop.Infrastructure.External.Media;

internal sealed class VideoAnalyzer : IVideoAnalyzer
{
    private readonly ILogger<VideoAnalyzer> _logger;
    private static bool _ffmpegInitialized = false;
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1); // لمنع تعارض التحميل لو أكتر من ريكويست دخلوا مع بعض

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
                await _semaphore.WaitAsync(cancellationToken);
                try
                {
                    if (!_ffmpegInitialized)
                    {
                        var currentDir = Directory.GetCurrentDirectory();

                        // تحميل ملفات FFmpeg في المسار الحالي
                        await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, currentDir);

                        // إجبار المكتبة تقرأ من المسار ده صراحةً
                        FFmpeg.SetExecutablesPath(currentDir);

                        _ffmpegInitialized = true;
                    }
                }
                finally
                {
                    _semaphore.Release();
                }
            }

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