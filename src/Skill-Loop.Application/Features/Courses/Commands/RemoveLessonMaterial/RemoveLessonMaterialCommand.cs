using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Courses.Common;
using System;

namespace Skill_Loop.Application.Features.Courses.Commands.RemoveLessonMaterial;

public sealed record RemoveLessonMaterialCommand(
    Guid CourseId, 
    Guid SectionId, 
    Guid LessonId, 
    Guid MaterialId) : ICommand, ICourseCommand;
