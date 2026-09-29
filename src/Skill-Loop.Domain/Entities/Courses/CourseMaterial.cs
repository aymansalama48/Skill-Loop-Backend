using Skill_Loop.Domain.Common.Entities;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Domain.Entities.Courses;

public class CourseMaterial : AuditableEntity
{
    public Guid CourseId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string DriveFileId { get; set; } = string.Empty;
    public string? DriveFolderId { get; set; }
    public int SortOrder { get; set; }
    public Guid UploadedByUserId { get; set; }
    public MaterialType? MaterialType { get; set; }
    
    public Course? Course { get; set; }

    public static CourseMaterial Create(
        Guid courseId,
        string fileName,
        string mimeType,
        long sizeBytes,
        string driveFileId,
        string? driveFolderId,
        int sortOrder,
        Guid uploadedByUserId,
        MaterialType? materialType = null)
    {
        return new CourseMaterial
        {
            CourseId = courseId,
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
