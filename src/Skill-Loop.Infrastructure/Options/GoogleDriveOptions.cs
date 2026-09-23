namespace Skill_Loop.Infrastructure.Options;

public sealed class GoogleDriveOptions
{
    public const string SectionName = "GoogleDrive";
    public string RootFolderId { get; set; } = string.Empty;
    public string ServiceAccountFilePath { get; set; } = string.Empty;
    public long MaxFileSizeBytes { get; set; } = 536870912;
    public List<string> AllowedMimeTypes { get; set; } = [];
    public double QuotaWarningThreshold { get; set; } = 0.9;
}