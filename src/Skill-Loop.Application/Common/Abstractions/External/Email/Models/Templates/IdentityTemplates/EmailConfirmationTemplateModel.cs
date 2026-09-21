using Skill_Loop.Application.Common.Abstractions.External.Email.Models.Templates.Base;

namespace Skill_Loop.Application.Common.Abstractions.External.Email.Models.Templates.IdentityTemplates;

public class EmailConfirmationTemplateModel : BaseEmailTemplateModel
{
    public string ConfirmationLink { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string Device { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}