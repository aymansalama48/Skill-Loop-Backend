namespace Skill_Loop.Application.Features.Accounts.StaffAuth.Shared;

/// <summary>
/// استجابة المصادقة الخاصة بالموظفين (تحتوي على الـ Tokens وبيانات المستخدم)
/// </summary>
public record StaffAuthResponse
{
    /// <summary>
    /// معرف المستخدم
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// البريد الإلكتروني
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// الاسم الكامل
    /// </summary>
    public string FullName { get; init; } = string.Empty;

    /// <summary>
    /// Access Token (JWT)
    /// </summary>
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>
    /// Refresh Token
    /// </summary>
    public string RefreshToken { get; init; } = string.Empty;

    /// <summary>
    /// مدة صلاحية الـ Access Token بالثواني (افتراضي 3600 = ساعة)
    /// </summary>
    public int ExpiresInSeconds { get; init; } 

    /// <summary>
    /// تاريخ ووقت تسجيل الدخول
    /// </summary>
    public DateTime LoggedInAt { get; init; }

    /// <summary>
    /// قائمة الأدوار (للعرض أو الاستخدام)
    /// </summary>
    public List<string> Roles { get; init; } = new();

}