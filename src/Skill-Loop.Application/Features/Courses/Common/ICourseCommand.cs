using System;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Courses.Common;

/// <summary>
/// Interface for commands that modify an existing course.
/// Used by CourseOwnershipBehavior to ensure the user owns the course.
/// </summary>
public interface ICourseCommand
{
    Guid CourseId { get; }
}
