namespace Skill_Loop.Application.Features.Accounts.UserAuth.Shared;

/// <summary>
/// استجابة المصادقة الخاصة بالمستخدم العادي / الطالب
/// </summary>
public record UserAuthResponse
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public int ExpiresInSeconds { get; init; }
    public DateTime LoggedInAt { get; init; }
    public List<string> Roles { get; init; } = new();

}