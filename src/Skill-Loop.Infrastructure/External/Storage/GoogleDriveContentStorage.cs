using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Upload;
using Microsoft.Extensions.Options;
using Skill_Loop.Application.Common.Abstractions.External.Storage;
using Skill_Loop.Application.Common.Models.Storage;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Infrastructure.Options;

namespace Skill_Loop.Infrastructure.External.Storage;

internal sealed class GoogleDriveContentStorage(IOptions<GoogleDriveOptions> options) : ICourseContentStorage
{
    private readonly GoogleDriveOptions _options = options.Value;

    public async Task<Result<CourseContentUploadResult>> UploadAsync(
        Stream fileStream,
        string fileName,
        string folderName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var credential = await GoogleCredential.FromFileAsync(_options.ServiceAccountFilePath, cancellationToken);
            if (credential.IsCreateScopedRequired)
            {
                credential = credential.CreateScoped(DriveService.Scope.Drive);
            }

            var service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Skill-Loop",
            });

            var folderId = await EnsureSessionFolderInternalAsync(service, folderName, cancellationToken);

            var fileMetadata = new Google.Apis.Drive.v3.Data.File
            {
                Name = fileName,
                Parents = [folderId],
            };

            using var stream = fileStream;
            var request = service.Files.Create(fileMetadata, stream, "application/octet-stream");
            request.Fields = "id,parents";
            var upload = await request.UploadAsync(cancellationToken);

            if (upload.Status != UploadStatus.Completed)
            {
                return Result<CourseContentUploadResult>.Failure(new Error("UPLOAD_FAILED", "Google Drive upload failed.", ErrorType.Failure));
            }

            return Result<CourseContentUploadResult>.Success(new CourseContentUploadResult(
                request.ResponseBody?.Id ?? string.Empty,
                folderId,
                fileStream.Length));
        }
        catch (Exception ex)
        {
            return Result<CourseContentUploadResult>.Failure(new Error("UPLOAD_FAILED", ex.Message, ErrorType.Failure));
        }
    }

    public async Task<Result<CourseContentDownloadResult>> DownloadAsync(
        string fileId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var credential = await GoogleCredential.FromFileAsync(_options.ServiceAccountFilePath, cancellationToken);
            if (credential.IsCreateScopedRequired)
            {
                credential = credential.CreateScoped(DriveService.Scope.Drive);
            }

            var service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Skill-Loop",
            });

            var request = service.Files.Get(fileId);
            var stream = new MemoryStream();
            await request.DownloadAsync(stream, cancellationToken);
            stream.Position = 0;

            var file = await service.Files.Get(fileId).ExecuteAsync(cancellationToken);

            return Result<CourseContentDownloadResult>.Success(new CourseContentDownloadResult(
                stream,
                file?.Name ?? "download",
                file?.MimeType ?? "application/octet-stream",
                stream.Length));
        }
        catch (Exception ex)
        {
            return Result<CourseContentDownloadResult>.Failure(new Error("DOWNLOAD_FAILED", ex.Message, ErrorType.Failure));
        }
    }

    public async Task<Result> DeleteAsync(
        string fileId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var credential = await GoogleCredential.FromFileAsync(_options.ServiceAccountFilePath, cancellationToken);
            if (credential.IsCreateScopedRequired)
            {
                credential = credential.CreateScoped(DriveService.Scope.Drive);
            }

            var service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Skill-Loop",
            });

            await service.Files.Delete(fileId).ExecuteAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("DELETE_FAILED", ex.Message, ErrorType.Failure));
        }
    }

    public async Task<Result<DriveQuotaUsage>> GetQuotaUsageAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var credential = await GoogleCredential.FromFileAsync(_options.ServiceAccountFilePath, cancellationToken);
            if (credential.IsCreateScopedRequired)
            {
                credential = credential.CreateScoped(DriveService.Scope.Drive);
            }

            var service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Skill-Loop",
            });

            var about = await service.About.Get().ExecuteAsync(cancellationToken);

            return Result<DriveQuotaUsage>.Success(new DriveQuotaUsage(
                about.StorageQuota?.Usage ?? 0,
                about.StorageQuota?.Limit ?? 0));
        }
        catch (Exception ex)
        {
            return Result<DriveQuotaUsage>.Failure(new Error("QUOTA_CHECK_FAILED", ex.Message, ErrorType.Failure));
        }
    }

    public async Task<Result<string>> EnsureSessionFolderAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var credential = await GoogleCredential.FromFileAsync(_options.ServiceAccountFilePath, cancellationToken);
            if (credential.IsCreateScopedRequired)
            {
                credential = credential.CreateScoped(DriveService.Scope.Drive);
            }

            var service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Skill-Loop",
            });

            return Result<string>.Success(await EnsureSessionFolderInternalAsync(service, sessionId.ToString(), cancellationToken));
        }
        catch (Exception ex)
        {
            return Result<string>.Failure(new Error("FOLDER_CREATE_FAILED", ex.Message, ErrorType.Failure));
        }
    }

    private async Task<string> EnsureSessionFolderInternalAsync(
        DriveService service,
        string folderName,
        CancellationToken cancellationToken)
    {
        var query = $"mimeType='application/vnd.google-apps.folder' and name='{folderName}' and trashed=false";
        var listRequest = new FilesResource.ListRequest(service) { Q = query };
        var files = await listRequest.ExecuteAsync(cancellationToken);

        if (files.Files?.Count > 0)
        {
            return files.Files[0].Id!;
        }

        var folderMetadata = new Google.Apis.Drive.v3.Data.File
        {
            Name = folderName,
            MimeType = "application/vnd.google-apps.folder",
            Parents = [string.IsNullOrEmpty(_options.RootFolderId) ? "root" : _options.RootFolderId],
        };

        var folder = await service.Files.Create(folderMetadata).ExecuteAsync(cancellationToken);
        return folder.Id!;
    }
}
