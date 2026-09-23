using Skill_Loop.Domain.Common.Entities;
using SessionType = Skill_Loop.Domain.Entities.Session.Session;
using Skill_Loop.Domain.Entities.SessionMaterial.Events;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Domain.Entities.SessionMaterial;

public class SessionMaterial : AuditableEntity
{
    public Guid SessionId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string DriveFileId { get; set; } = string.Empty;
    public string? DriveFolderId { get; set; }
    public int SortOrder { get; set; }
    public Guid UploadedByUserId { get; set; }
    public MaterialType? MaterialType { get; set; }

    public SessionType? Session { get; set; }

    public static SessionMaterial Create(
        Guid sessionId,
        string fileName,
        string mimeType,
        long sizeBytes,
        string driveFileId,
        string? driveFolderId,
        int sortOrder,
        Guid uploadedByUserId,
        MaterialType? materialType = null)
    {
        var material = new SessionMaterial
        {
            SessionId = sessionId,
            FileName = fileName,
            MimeType = mimeType,
            SizeBytes = sizeBytes,
            DriveFileId = driveFileId,
            DriveFolderId = driveFolderId,
            SortOrder = sortOrder,
            UploadedByUserId = uploadedByUserId,
            MaterialType = materialType
        };

        material.AddDomainEvent(new SessionMaterialUploadedEvent(material));

        return material;
    }
}
