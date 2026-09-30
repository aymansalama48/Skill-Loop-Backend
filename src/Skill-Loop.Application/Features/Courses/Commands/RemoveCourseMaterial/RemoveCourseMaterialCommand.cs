using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Courses.Common;
using System;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Courses.Commands.RemoveCourseMaterial;

[AuthenticatedOnly]
public sealed record RemoveCourseMaterialCommand(Guid CourseId, Guid MaterialId) : ICommand, ICourseCommand;
