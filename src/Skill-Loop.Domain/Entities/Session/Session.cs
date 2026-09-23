using Skill_Loop.Domain.Common.Entities;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Domain.Entities.Session;

public class Session : AuditableEntity
{
    public Guid InstructorId { get; set; }
    public Guid OwnerId { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.Draft;
    public string Title { get; set; } = string.Empty;

    public ICollection<Skill_Loop.Domain.Entities.SessionMaterial.SessionMaterial> Materials { get; set; } = new List<Skill_Loop.Domain.Entities.SessionMaterial.SessionMaterial>();
}
