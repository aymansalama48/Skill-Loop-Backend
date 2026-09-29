namespace Skill_Loop.Application.Features.Sessions;

/// <summary>
/// حدود-validation مشتركة لميزات الـ Sessions (الحجز + الجدولة)
/// </summary>
public static class SessionDefaults
{
    public const int DurationMinutes = 60;
    public const int MaxDurationMinutes = 480;
    public const int MaxParticipants = 50;
    public const int MaxTitleLength = 200;
    public const int MaxDescriptionLength = 2000;
    public const int MaxLocationDetailsLength = 500;
}
