namespace Skill_Loop.Application.Common.Abstractions.External.Email.Models.Templates.SessionsTemplates;

using Skill_Loop.Application.Common.Abstractions.External.Email.Models.Templates.Base;

public sealed class StorageQuotaWarningTemplateModel : BaseEmailTemplateModel
{
    public long UsedBytes { get; set; }
    public long TotalBytes { get; set; }
    public double Threshold { get; set; }
    public string UsedFormatted { get; set; } = string.Empty;
    public string TotalFormatted { get; set; } = string.Empty;
}