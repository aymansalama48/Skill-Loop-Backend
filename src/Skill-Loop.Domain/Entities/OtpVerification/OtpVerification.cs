using Skill_Loop.Domain.Common.Entities;
using Skill_Loop.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Skill_Loop.Domain.Entities.OtpVerification
{
    /// <summary>
    /// سجل مستقل للتحقق عبر OTP
    /// </summary>
    public class OtpVerification : BaseEntity
    {
        public string Phone { get; set; } = string.Empty;

        /// الكود بيتخزن Hashed دايمًا (HMAC)، مش نص عادي — عشان حتى لو حد وصل للداتابيز میقدرش يستخدمه
        public string CodeHash { get; set; } = string.Empty;

        public DateTime Expiry { get; set; }
        public DateTime NextResendAllowedAtUtc { get; set; }

        public OtpPurpose Purpose { get; set; } = OtpPurpose.AppointmentBooking;

        /// اتستهلك = نجح التحقق، أو اتقفل بسبب Max Attempts، أو اتلغى لصدور كود جديد بدله
        public bool IsConsumed { get; set; } = false;
        public DateTime? VerifiedAt { get; set; }

        public int AttemptsCount { get; set; } = 0;
        public int MaxAttempts { get; set; } = 5;
        public bool IsMaxAttemptsReached => AttemptsCount >= MaxAttempts;

    }
}
