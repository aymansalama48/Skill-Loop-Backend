using System;
using System.Collections.Generic;
using System.Text;

namespace Skill_Loop.Domain.Enums
{
    /// <summary>
    /// الغرض من التحقق عبر OTP
    /// </summary>
    public enum OtpPurpose
    {
        VerifyPhone,          // تغيير رقم الهاتف أو تأكيده
        EmailVerification,    // 👈 تأكيد البريد الإلكتروني (تسجيل حساب جديد)
        PasswordReset         // 👈 إعادة تعيين كلمة المرور
    }
}
