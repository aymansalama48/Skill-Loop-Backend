namespace Skill_Loop.Application.Common.Models.Storage;

public sealed record CourseContentUploadResult(
    string DriveFileId,
    string DriveFolderId,
    long SizeBytes);

public sealed record CourseContentDownloadResult(
    Stream Stream,
    string FileName,
    string MimeType,
    long SizeBytes);

public sealed record DriveQuotaUsage(
    long UsedBytes,
    long TotalBytes);