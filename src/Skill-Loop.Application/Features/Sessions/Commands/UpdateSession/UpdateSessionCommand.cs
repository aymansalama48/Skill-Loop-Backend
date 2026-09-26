using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Application.Features.Sessions.Commands.UpdateSession;

/// <summary>
/// تحديث بيانات الجلسة. كل الـ scheduling parameters اختيارية (Partial Update):
/// أي معامل مش مبعوت هيفضل زي ما هو.
/// </summary>
public sealed record UpdateSessionCommand(
    Guid Id,
    string Title,
    string? Description = null,
    DateTime? ScheduledAtUtc = null,
    int? DurationMinutes = null,
    int? CreditsPrice = null,
    SessionLocationType? LocationType = null,
    string? LocationDetails = null,
    int? MaxParticipants = null) : ICommand, ICacheInvalidatorCommand
{
    public IReadOnlyCollection<string> CacheKeys =>
    [
        "sessions:all",
        $"sessions:{Id}"
    ];
}
