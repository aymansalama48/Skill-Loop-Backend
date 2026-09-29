using System;

namespace Skill_Loop.Application.Features.Courses.Common;

/// <summary>
/// Interface for commands that modify an existing course.
/// Used by CourseOwnershipBehavior to ensure the user owns the course.
/// </summary>
public interface ICourseCommand
{
    Guid CourseId { get; }
}
