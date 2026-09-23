using Skill_Loop.Domain.Common.Events;

namespace Skill_Loop.Domain.Entities.SessionMaterial.Events;

public sealed record SessionMaterialUploadedEvent(SessionMaterial Material) : IDomainEvent;
