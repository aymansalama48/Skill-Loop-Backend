using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
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

    private async Task<DriveService> GetDriveServiceAsync(CancellationToken cancellationToken)
    {
        var credential = await GoogleCredential.FromFileAsync(_options.ServiceAccountFilePath, cancellationToken);
        if (credential.IsCreateScopedRequired)
        {
            credential = credential.CreateScoped(DriveService.Scope.Drive);
        }

        return new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "Skill-Loop",
        });
    }

    public async Task<Result<CourseContentUploadResult>> UploadAsync(
        Stream fileStream,
        string fileName,
        string folderId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var service = await GetDriveServiceAsync(cancellationToken);

            if (fileStream.CanSeek)
            {
                fileStream.Position = 0;
            }

            var fileMetadata = new Google.Apis.Drive.v3.Data.File
            {
                Name = fileName,
                Parents = [folderId],
            };

            var request = service.Files.Create(fileMetadata, fileStream, "application/octet-stream");
            request.Fields = "id,parents";

            // السطر الأهم لدعم حسابات الـ Workspace والمساحات المشتركة
            request.SupportsAllDrives = true;

            var upload = await request.UploadAsync(cancellationToken);

            if (upload.Status != UploadStatus.Completed)
            {
                var errorMessage = upload.Exception != null
                    ? upload.Exception.Message
                    : "Google Drive upload failed for an unknown reason.";

                return Result<CourseContentUploadResult>.Failure(new Error("UPLOAD_FAILED", errorMessage, ErrorType.Failure));
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
            var service = await GetDriveServiceAsync(cancellationToken);

            var request = service.Files.Get(fileId);
            request.SupportsAllDrives = true; // دعم المساحات المشتركة

            var stream = new MemoryStream();
            await request.DownloadAsync(stream, cancellationToken);
            stream.Position = 0;

            var fileRequest = service.Files.Get(fileId);
            fileRequest.Fields = "name,mimeType";
            fileRequest.SupportsAllDrives = true; // دعم المساحات المشتركة
            var file = await fileRequest.ExecuteAsync(cancellationToken);

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
            var service = await GetDriveServiceAsync(cancellationToken);
            var request = service.Files.Delete(fileId);
            request.SupportsAllDrives = true; // دعم المساحات المشتركة
            await request.ExecuteAsync(cancellationToken);
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
            var service = await GetDriveServiceAsync(cancellationToken);

            var request = service.About.Get();
            request.Fields = "storageQuota";
            var about = await request.ExecuteAsync(cancellationToken);

            long usage = about.StorageQuota?.Usage ?? 0;
            long limit = about.StorageQuota?.Limit ?? 0;

            // في الـ Shared Drives غالباً الـ limit بيرجع صفر، فهنفترض مساحة ضخمة (1 تيرا بايت) لتخطي الهاندلر
            if (limit == 0)
            {
                limit = 1000L * 1024 * 1024 * 1024; // 1 TB
            }

            return Result<DriveQuotaUsage>.Success(new DriveQuotaUsage(usage, limit));
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
            var service = await GetDriveServiceAsync(cancellationToken);

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
        listRequest.Fields = "files(id)";
        listRequest.SupportsAllDrives = true; // دعم المساحات المشتركة
        listRequest.IncludeItemsFromAllDrives = true; // السماح بالبحث داخلها

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

        var createRequest = service.Files.Create(folderMetadata);
        createRequest.Fields = "id";
        createRequest.SupportsAllDrives = true; // دعم المساحات المشتركة
        var folder = await createRequest.ExecuteAsync(cancellationToken);

        return folder.Id!;
    }
}