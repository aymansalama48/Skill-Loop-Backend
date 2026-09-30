using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Courses.Common;
using System;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Courses.Commands.RemoveLessonMaterial;

[AuthenticatedOnly]
public sealed record RemoveLessonMaterialCommand(
    Guid CourseId, 
    Guid SectionId, 
    Guid LessonId, 
    Guid MaterialId) : ICommand, ICourseCommand;
