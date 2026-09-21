namespace Skill_Loop.Infrastructure.Options;

public class OtpOptions
{
    public const string SectionName = "OtpSettings";

    public int CodeLength { get; set; } = 6;
    public TimeSpan Expiry { get; set; } = TimeSpan.FromMinutes(5);
    public TimeSpan ResendCooldown { get; set; } = TimeSpan.FromSeconds(60);
    public int MaxAttempts { get; set; } = 5;

    /// سر ثابت من appsettings/Secret Manager — بيتضاف على الكود قبل الـ Hashing (Pepper)
    public string HashingSecret { get; set; } = string.Empty;
}

