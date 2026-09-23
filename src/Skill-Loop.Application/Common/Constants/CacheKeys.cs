namespace Skill_Loop.Application.Common.Constants;

public static class CacheKeys
{
    // Session Materials
    public static string SessionMaterials(Guid sessionId) => $"session-materials-{sessionId}";
    public static string SessionMaterialsAll => "session-materials-all";
    public static string SessionMaterial(Guid materialId) => $"session-material-{materialId}";
    public static string SessionMaterialDownload(Guid sessionId, Guid materialId) => $"session-material-download-{sessionId}-{materialId}";
    public static string SessionMaterialsPaged(Guid sessionId, int pageNumber, int pageSize) => $"session-materials-{sessionId}-page-{pageNumber}-size-{pageSize}";
    
    // Drive Quota
    public const string DriveQuotaUsage = "drive-quota-usage";
    
    // Users
    public const string UsersList = "users-list";
}