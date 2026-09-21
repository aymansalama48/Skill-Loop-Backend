using Skill_Loop.Domain.Common.Entities;
using Skill_Loop.Domain.Entities.Invitation.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Skill_Loop.Domain.Entities.Invitation
{
    public class StaffInvitation : AuditableEntity
    {
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // Admin, Doctor, Receptionist
        public string Token { get; set; } = string.Empty; // Secure Random Token
        public DateTime ExpiresAtUtc { get; set; }
        public bool IsUsed { get; set; }
        public DateTime? UsedAtUtc { get; set; }

        public Guid AdminId { get; set; } // الأدمن الذي أرسل الدعوة
        public string AdminName { get; set; } = string.Empty; // اسم الأدمن الذي أرسل الدعوة



        /// <summary>
        /// Factory Method لإنشاء الكيان وتأمين تسجيل الـ Event بداخله
        /// </summary>
        public static StaffInvitation Create(
            string email,
            string role,
            string token,
            DateTime expiresAtUtc,
            Guid adminId,
            string adminName)
        {
            var invitation = new StaffInvitation
            {
                Id = Guid.CreateVersion7(),
                Email = email,
                Role = role,
                Token = token,
                ExpiresAtUtc = expiresAtUtc,
                IsUsed = false,
                AdminId = adminId,
                AdminName = adminName
            };

            // تسجيل حدث إنشاء الدعوة داخل الكيان
            invitation.AddDomainEvent(new StaffInvitationCreatedEvent(invitation));

            return invitation;
        }
    }
}
