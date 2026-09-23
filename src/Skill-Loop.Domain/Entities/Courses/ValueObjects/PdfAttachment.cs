using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Domain.Entities.Courses.ValueObjects;

public sealed record PdfAttachment
{
    public string FileName { get; private init; } = string.Empty;
    public string StorageUrl { get; private init; } = string.Empty;
    public long FileSizeBytes { get; private init; }

    private PdfAttachment() { }

    private PdfAttachment(string fileName, string storageUrl, long fileSizeBytes)
    {
        FileName = fileName;
        StorageUrl = storageUrl;
        FileSizeBytes = fileSizeBytes;
    }

    public static Result<PdfAttachment> Create(string fileName, string storageUrl, long fileSizeBytes)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return Result<PdfAttachment>.Failure(new Error("PdfAttachment.EmptyFileName", "File name is required.", ErrorType.Validation));
        }

        if (string.IsNullOrWhiteSpace(storageUrl))
        {
            return Result<PdfAttachment>.Failure(new Error("PdfAttachment.EmptyUrl", "Storage URL is required.", ErrorType.Validation));
        }

        if (fileSizeBytes <= 0)
        {
            return Result<PdfAttachment>.Failure(new Error("PdfAttachment.InvalidSize", "File size must be positive.", ErrorType.Validation));
        }

        return Result<PdfAttachment>.Success(new PdfAttachment(fileName.Trim(), storageUrl.Trim(), fileSizeBytes));
    }
}
