using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Courses.Common;
using System;

namespace Skill_Loop.Application.Features.Courses.Commands.RemoveCourseMaterial;

public sealed record RemoveCourseMaterialCommand(Guid CourseId, Guid MaterialId) : ICommand, ICourseCommand;
