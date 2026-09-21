using Skill_Loop.Application.Common.Abstractions.External.Email.Models.Templates.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace Skill_Loop.Application.Common.Abstractions.External.Email.Models.Templates.IdentityTemplates
{
    public class StaffInvitationTemplateModel : BaseEmailTemplateModel
    {
        public string RoleName { get; set; } = string.Empty;
        public string AdminName { get; set; } = string.Empty;
        public string InvitedEmail { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string InvitationLink { get; set; } = string.Empty;
        public int ExpiryHours { get; set; }

    }
}
