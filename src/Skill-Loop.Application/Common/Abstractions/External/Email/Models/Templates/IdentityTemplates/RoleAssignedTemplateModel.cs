using Skill_Loop.Application.Common.Abstractions.External.Email.Models.Templates.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace Skill_Loop.Application.Common.Abstractions.External.Email.Models.Templates.IdentityTemplates
{
    public class RoleAssignedTemplateModel : BaseEmailTemplateModel
    {
        public string RoleName { get; set; } = string.Empty;
        public string DashboardUrl { get; set; } = string.Empty;
    }
}
