using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Errors.Files;

/// <summary>
/// يجمع أخطاء إدارة الملفات المرفقة (صور الأشعة، التقارير الطبية، إلخ).
/// </summary>
public static class FileErrors
{
    public static readonly Error FileNotFound = new(
        "FILE_NOT_FOUND",
        "الملف المطلوب غير موجود.",
        ErrorType.NotFound);

    public static readonly Error EmptyFile = new(
        "FILE_EMPTY",
        "الملف المرفوع فارغ.",
        ErrorType.Validation);

    public static readonly Error InvalidFileName = new(
        "FILE_INVALID_NAME",
        "اسم الملف غير صالح أو فارغ.",
        ErrorType.Validation);

    public static readonly Error InvalidFolder = new(
        "FILE_INVALID_FOLDER",
        "مسار المجلد الهدف غير صالح أو غير موجود.",
        ErrorType.Validation);

    public static readonly Error FileTooLarge = new(
        "FILE_TOO_LARGE",
        "حجم الملف يتجاوز الحد الأقصى المسموح به.",
        ErrorType.Validation);

    public static readonly Error UnsupportedExtension = new(
        "FILE_UNSUPPORTED_EXTENSION",
        "نوع أو امتداد الملف غير مدعوم.",
        ErrorType.Validation);

    public static readonly Error FileAlreadyExists = new(
        "FILE_ALREADY_EXISTS",
        "يوجد ملف بنفس الاسم بالفعل.",
        ErrorType.Conflict);

    public static readonly Error UploadFailed = new(
        "FILE_UPLOAD_FAILED",
        "حدث خطأ غير متوقع أثناء رفع الملف.",
        ErrorType.Failure);
}