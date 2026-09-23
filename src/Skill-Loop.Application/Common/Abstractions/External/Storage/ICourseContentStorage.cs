using Skill_Loop.Application.Common.Models.Storage;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Abstractions.External.Storage;

public interface ICourseContentStorage
{
    Task<Result<CourseContentUploadResult>> UploadAsync(
        Stream fileStream,
        string fileName,
        string folderName,
        CancellationToken cancellationToken = default);

    Task<Result<CourseContentDownloadResult>> DownloadAsync(
        string fileId,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(
        string fileId,
        CancellationToken cancellationToken = default);

    Task<Result<DriveQuotaUsage>> GetQuotaUsageAsync(
        CancellationToken cancellationToken = default);

    Task<Result<string>> EnsureSessionFolderAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);
}