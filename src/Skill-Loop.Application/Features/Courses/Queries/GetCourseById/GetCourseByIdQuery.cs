using Skill_Loop.Application.Common.Constants;
using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Courses.DTOs;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Courses.Queries.GetCourseById;

[AllowAnonymous]
public sealed record GetCourseByIdQuery(Guid CourseId) : IQuery<CourseDetailDto>;
