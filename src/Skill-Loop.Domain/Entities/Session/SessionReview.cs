using Skill_Loop.Domain.Common.Entities;

namespace Skill_Loop.Domain.Entities.Sessions;

public sealed class SessionReview : AuditableEntity
{
    public Guid SessionId { get; private set; }
    public Guid UserId { get; private set; }
    public int Stars { get; private set; }
    public string? Comment { get; private set; }

    private SessionReview() { }

    internal static SessionReview Create(Guid sessionId, Guid userId, int stars, string? comment) =>
        new()
        {
            Id = Guid.CreateVersion7(),
            SessionId = sessionId,
            UserId = userId,
            Stars = stars,
            Comment = comment?.Trim()
        };
}
