using Skill_Loop.Application.Common.Abstractions.External.Email.Models.Templates.Base;

namespace Skill_Loop.Application.Common.Abstractions.External.Email.Models.Templates.IdentityTemplates;

public class WelcomeTemplateModel : BaseEmailTemplateModel
{
    public string LoginUrl { get; set; } = string.Empty;
}