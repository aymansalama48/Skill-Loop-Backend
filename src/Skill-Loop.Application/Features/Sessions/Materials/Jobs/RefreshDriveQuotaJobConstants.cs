namespace Skill_Loop.Application.Features.Sessions.Materials.Jobs;

public static class RefreshDriveQuotaJobConstants
{
    public const string JobId = "refresh-drive-quota";
    public const string QueueName = "default";
    public const string CronExpression = "0 */15 * * * *"; // Every 15 minutes
    
    // Cache keys
    public const string QuotaCacheKey = "drive-quota-usage";
    public const string QuotaCacheSlidingExpirationMinutes = "10";
    public const string QuotaCacheAbsoluteExpirationHours = "1";
    
    // Safety margin for uploads (100 MB)
    public const long SafetyMarginBytes = 100 * 1024 * 1024;
    
    // Per-session material limit
    public const int MaxMaterialsPerSession = 50;
    
    // Per-instructor total size cap (10 GB)
    public const long MaxTotalSizePerInstructorBytes = 10L * 1024 * 1024 * 1024;
    
    // Per-session total size cap (2 GB)
    public const long MaxTotalSizePerSessionBytes = 2L * 1024 * 1024 * 1024;
}