using FluentValidation;
using Skill_Loop.Application.Common.Errors.Sessions;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Application.Features.Sessions.Materials.Commands.UploadSessionMaterial;

public sealed class UploadSessionMaterialCommandValidator : AbstractValidator<UploadSessionMaterialCommand>
{
    private static readonly string[] AllowedMimeTypes =
    [
        "application/pdf",
        "video/mp4",
        "video/webm",
        "video/quicktime",
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/webp",
        "image/svg+xml",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/vnd.ms-powerpoint",
        "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        "text/plain",
        "application/zip",
        "application/x-rar-compressed",
        "application/x-7z-compressed"
    ];

    private const long MaxFileSizeBytes = 500 * 1024 * 1024; // 500 MB
    private const int MaxMaterialsPerSession = 50;
    private const long MaxTotalSizePerInstructorBytes = 10L * 1024 * 1024 * 1024; // 10 GB
    private const long MaxTotalSizePerSessionBytes = 2L * 1024 * 1024 * 1024; // 2 GB

    public UploadSessionMaterialCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Session ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Invalid Session ID.");

        RuleFor(x => x.FileStream)
            .NotNull().WithMessage("File stream is required.")
            .Must(s => s != null && s.Length > 0).WithMessage("File stream cannot be empty.");

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required.")
            .MaximumLength(255).WithMessage("File name cannot exceed 255 characters.")
            .Must(HaveValidExtension).WithMessage("File extension is not allowed.");

        RuleFor(x => x.MimeType)
            .NotEmpty().WithMessage("MIME type is required.")
            .Must(BeAllowedMimeType).WithMessage("File type is not supported.");

        RuleFor(x => x.SizeBytes)
            .GreaterThan(0).WithMessage("File size must be greater than zero.")
            .LessThanOrEqualTo(MaxFileSizeBytes).WithMessage($"File size cannot exceed {MaxFileSizeBytes / (1024 * 1024)} MB.");

        RuleFor(x => x.MaterialType)
            .IsInEnum().When(x => x.MaterialType.HasValue).WithMessage("Invalid material type.");
    }

    private static bool HaveValidExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var allowedExtensions = new[]
        {
            ".pdf",
            ".mp4", ".webm", ".mov",
            ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg",
            ".doc", ".docx",
            ".xls", ".xlsx",
            ".ppt", ".pptx",
            ".txt",
            ".zip", ".rar", ".7z"
        };
        return allowedExtensions.Contains(extension);
    }

    private static bool BeAllowedMimeType(string mimeType)
    {
        return AllowedMimeTypes.Contains(mimeType.ToLowerInvariant());
    }
}