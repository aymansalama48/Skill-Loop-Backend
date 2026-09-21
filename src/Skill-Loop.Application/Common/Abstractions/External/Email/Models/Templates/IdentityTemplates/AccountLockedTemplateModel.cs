using Skill_Loop.Application.Common.Abstractions.External.Email.Models.Templates.Base;

namespace Skill_Loop.Application.Common.Abstractions.External.Email.Models.Templates.IdentityTemplates;

public class AccountLockedTemplateModel : BaseEmailTemplateModel
{
    public string UnlockUrl { get; set; } = string.Empty;
}