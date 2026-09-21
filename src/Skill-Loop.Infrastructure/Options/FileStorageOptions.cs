namespace Skill_Loop.Infrastructure.Options;

/// <summary>
/// إعدادات تخزين الملفات.
/// </summary>
public sealed class FileStorageOptions
{

    public const string SectionName = "FileStorage";
    /// <summary>
    /// نوع مزود التخزين.
    /// Local / AzureBlob / S3 ...
    /// </summary>
    public string Provider { get; set; } = "Local";

    /// <summary>
    /// الفولدر الرئيسي لحفظ الملفات.
    /// </summary>
    public string RootFolder { get; set; } = "UploadedFiles";

    /// <summary>
    /// إنشاء الفولدر تلقائياً إذا لم يكن موجوداً.
    /// </summary>
    public bool CreateIfNotExists { get; set; } = true;

    /// <summary>
    /// أقصى حجم مسموح للملف (MB).
    /// </summary>
    public long MaxFileSizeInMB { get; set; } = 10;

    /// <summary>
    /// هل يتم إنشاء اسم فريد لكل ملف.
    /// </summary>
    public bool GenerateUniqueFileName { get; set; } = true;

    /// <summary>
    /// هل يتم الاحتفاظ بالاسم الأصلى للملف.
    /// </summary>
    public bool PreserveOriginalFileName { get; set; } = false;

    /// <summary>
    /// هل يسمح باستبدال الملفات الموجودة.
    /// </summary>
    public bool OverwriteExistingFiles { get; set; } = false;

    /// <summary>
    /// الامتدادات المسموح بها.
    /// </summary>
    public List<string> AllowedExtensions { get; set; } =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".gif",
        ".bmp",
        ".webp",
        ".svg",

        ".pdf",

        ".doc",
        ".docx",

        ".xls",
        ".xlsx",

        ".ppt",
        ".pptx",

        ".txt",

        ".zip",
        ".rar",
        ".7z"
    ];
}
