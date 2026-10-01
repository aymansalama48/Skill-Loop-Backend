using System;
using System.Threading;
using System.Threading.Tasks;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Abstractions.External.Media;

public record VideoMetadata(TimeSpan Duration, string Resolution);

public interface IVideoAnalyzer
{
    Task<Result<VideoMetadata>> AnalyzeVideoAsync(string videoUrlOrPath, CancellationToken cancellationToken = default);
}
