using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Courses.DTOs;

namespace Skill_Loop.Application.Features.Courses.Queries.GetCourseById;

public sealed record GetCourseByIdQuery(Guid CourseId) : ICacheableQuery<CourseDetailDto>
{
    public string CacheKey => $"courses:detail:{CourseId}";
    public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(30);
    public TimeSpan? AbsoluteExpiration => TimeSpan.FromHours(4);
}