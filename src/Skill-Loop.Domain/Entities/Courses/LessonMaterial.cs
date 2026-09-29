using Skill_Loop.Domain.Common.Entities;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Domain.Entities.Courses;

public class LessonMaterial : AuditableEntity
{
    public Guid LessonId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string DriveFileId { get; set; } = string.Empty;
    public string? DriveFolderId { get; set; }
    public int SortOrder { get; set; }
    public Guid UploadedByUserId { get; set; }
    public MaterialType? MaterialType { get; set; }
    
    public Lesson? Lesson { get; set; }

    public static LessonMaterial Create(
        Guid lessonId,
        string fileName,
        string mimeType,
        long sizeBytes,
        string driveFileId,
        string? driveFolderId,
        int sortOrder,
        Guid uploadedByUserId,
        MaterialType? materialType = null)
    {
        return new LessonMaterial
        {
            LessonId = lessonId,
            FileName = fileName,
            MimeType = mimeType,
            SizeBytes = sizeBytes,
            DriveFileId = driveFileId,
            DriveFolderId = driveFolderId,
            SortOrder = sortOrder,
            UploadedByUserId = uploadedByUserId,
            MaterialType = materialType
        };
    }
}
