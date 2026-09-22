using Skill_Loop.Domain.Common.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Skill_Loop.Domain.Entities.SiteSettings
{
    public sealed class SiteSettings : BaseEntity
    {

        public string? AppName { get; set; } = string.Empty;
        public string? LogoName { get; set; } = string.Empty;

        public string? SupportEmail { get; set; } = string.Empty;

        public string? ContactPhoneNumber { get; set; } = string.Empty;

        public string? Address { get; set; } = string.Empty;

        public string? WebsiteUrl { get; set; } = string.Empty;

        public string? FacebookUrl { get; set; } = string.Empty;

        public string? InstagramUrl { get; set; } = string.Empty;

        public string? WhatsAppNumber { get; set; } = string.Empty;
    }
}
