using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Sessions.Common;
using Skill_Loop.Domain.Enums;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Sessions.Commands.UpdateSession;

/// <summary>
/// تحديث بيانات الجلسة. كل الـ scheduling parameters اختيارية (Partial Update):
/// أي معامل مش مبعوت هيفضل زي ما هو.
/// </summary>
[Permission(Permissions.Sessions.Update)]
public sealed record UpdateSessionCommand(
    Guid Id,
    string Title,
    string? Description = null,
    DateTime? ScheduledAtUtc = null,
    int? DurationMinutes = null,
    int? CreditsPrice = null,
    SessionLocationType? LocationType = null,
    string? LocationDetails = null,
    int? MaxParticipants = null) : ICommand, ICacheInvalidatorCommand, ISessionCommand
{
    public Guid SessionId => Id;

    public IReadOnlyCollection<string> CacheKeys =>
    [
        "sessions:all",
        $"sessions:{Id}"
    ];
}
