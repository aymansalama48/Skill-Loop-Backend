namespace Skill_Loop.UnitTests.Common;

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Skill_Loop.Application.Common.Abstractions.External.Storage;
using Skill_Loop.Application.Common.Models.Storage;
using Skill_Loop.Domain.Common.Results;

public class MockCourseContentStorage : ICourseContentStorage
{
    private readonly Result<CourseContentUploadResult> _uploadResult;
    private readonly Result<CourseContentDownloadResult> _downloadResult;
    private readonly Result _deleteResult;
    private readonly Result<DriveQuotaUsage> _quotaResult;
    private readonly Result<string> _folderResult;

    private MockCourseContentStorage(
        Result<CourseContentUploadResult> uploadResult,
        Result<CourseContentDownloadResult> downloadResult,
        Result deleteResult,
        Result<DriveQuotaUsage> quotaResult,
        Result<string> folderResult)
    {
        _uploadResult = uploadResult;
        _downloadResult = downloadResult;
        _deleteResult = deleteResult;
        _quotaResult = quotaResult;
        _folderResult = folderResult;
    }

    public static MockCourseContentStorage Success(
        CourseContentUploadResult uploadData = null,
        CourseContentDownloadResult downloadData = null,
        DriveQuotaUsage quota = default,
        string folderId = "folder-1")
    {
        return new MockCourseContentStorage(
            uploadResult: Result<CourseContentUploadResult>.Success(uploadData ?? new CourseContentUploadResult("file-1", folderId, 1024)),
            downloadResult: Result<CourseContentDownloadResult>.Success(downloadData ?? new CourseContentDownloadResult(new MemoryStream(), "test.txt", "text/plain", 1024)),
            deleteResult: Result.Success(),
            quotaResult: Result<DriveQuotaUsage>.Success(quota),
            folderResult: Result<string>.Success(folderId));
    }

    public static MockCourseContentStorage Failure(string errorMessage = "Storage failure")
    {
        return new MockCourseContentStorage(
            uploadResult: Result<CourseContentUploadResult>.Failure(errorMessage),
            downloadResult: Result<CourseContentDownloadResult>.Failure(errorMessage),
            deleteResult: Result.Failure(errorMessage),
            quotaResult: Result<DriveQuotaUsage>.Failure(errorMessage),
            folderResult: Result<string>.Failure(errorMessage));
    }

    public Task<Result<CourseContentUploadResult>> UploadAsync(Stream fileStream, string fileName, string folderName, CancellationToken cancellationToken = default)
        => Task.FromResult(_uploadResult);

    public Task<Result<CourseContentDownloadResult>> DownloadAsync(string fileId, CancellationToken cancellationToken = default)
        => Task.FromResult(_downloadResult);

    public Task<Result> DeleteAsync(string fileId, CancellationToken cancellationToken = default)
        => Task.FromResult(_deleteResult);

    public Task<Result<DriveQuotaUsage>> GetQuotaUsageAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_quotaResult);

    public Task<Result<string>> EnsureSessionFolderAsync(Guid sessionId, CancellationToken cancellationToken = default)
        => Task.FromResult(_folderResult);
}
