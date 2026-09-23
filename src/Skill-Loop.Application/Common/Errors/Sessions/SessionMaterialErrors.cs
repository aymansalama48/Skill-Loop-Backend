using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Errors.Sessions;

public static class SessionMaterialErrors
{
    public static readonly Error NotFound = new(
        "SESSION_MATERIAL_NOT_FOUND",
        "Session material not found.",
        ErrorType.NotFound);

    public static readonly Error NotOwner = new(
        "SESSION_MATERIAL_NOT_OWNER",
        "Only the session owner can perform this operation.",
        ErrorType.Forbidden);

    public static readonly Error FileTooLarge = new(
        "SESSION_MATERIAL_FILE_TOO_LARGE",
        "The uploaded file exceeds the configured size limit.",
        ErrorType.Validation);

    public static readonly Error UnsupportedFileType = new(
        "SESSION_MATERIAL_UNSUPPORTED_FILE_TYPE",
        "The uploaded file type is not supported.",
        ErrorType.Validation);

    public static readonly Error StorageQuotaExceeded = new(
        "SESSION_MATERIAL_STORAGE_QUOTA_EXCEEDED",
        "The projected storage usage exceeds the configured quota.",
        ErrorType.Conflict);

    public static readonly Error UploadFailed = new(
        "SESSION_MATERIAL_UPLOAD_FAILED",
        "The course material could not be uploaded.",
        ErrorType.Unexpected);

    public static readonly Error MaterialLimitReachedForSession = new(
        "SESSION_MATERIAL_LIMIT_REACHED_FOR_SESSION",
        "The session has reached its material limit.",
        ErrorType.Conflict);

    public static readonly Error NotAuthorizedToView = new(
        "SESSION_MATERIAL_NOT_AUTHORIZED_TO_VIEW",
        "You are not authorized to view this session material.",
        ErrorType.Forbidden);

    public static readonly Error SessionNotFound = new(
        "SESSION_NOT_FOUND",
        "Session not found.",
        ErrorType.NotFound);

    public static readonly Error SessionNotAvailable = new(
        "SESSION_NOT_AVAILABLE_FOR_MATERIALS",
        "Only draft and published sessions can contain materials.",
        ErrorType.Conflict);

    public static readonly Error DownloadFailed = new(
        "SESSION_MATERIAL_DOWNLOAD_FAILED",
        "The course material could not be downloaded.",
        ErrorType.Unexpected);

    public static readonly Error DeleteFailed = new(
        "SESSION_MATERIAL_DELETE_FAILED",
        "The course material could not be deleted.",
        ErrorType.Unexpected);

    public static readonly Error InvalidReorder = new(
        "SESSION_MATERIAL_INVALID_REORDER",
        "The requested material order is invalid.",
        ErrorType.Validation);
}
