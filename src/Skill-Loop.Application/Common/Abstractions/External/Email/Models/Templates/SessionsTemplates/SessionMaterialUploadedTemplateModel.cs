using Skill_Loop.Application.Common.Abstractions.External.Email.Models.Templates.Base;

namespace Skill_Loop.Application.Common.Abstractions.External.Email.Models.Templates.SessionsTemplates;

public sealed class SessionMaterialUploadedTemplateModel : BaseEmailTemplateModel
{
    public string SessionTitle { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public string MaterialType { get; set; } = string.Empty;
    public string UploadedAt { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public string InstructorName { get; set; } = string.Empty;
}