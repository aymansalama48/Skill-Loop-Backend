using Skill_Loop.Domain.Common.Events;

namespace Skill_Loop.Domain.Entities.Courses.Events;

public sealed record CourseCreatedDomainEvent(Guid CourseId, string Title) : IDomainEvent;
public sealed record CourseUpdatedDomainEvent(Guid CourseId, string Title) : IDomainEvent;
public sealed record CoursePublishedDomainEvent(Guid CourseId, string Title) : IDomainEvent;
