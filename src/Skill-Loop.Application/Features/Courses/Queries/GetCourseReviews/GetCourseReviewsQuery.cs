using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Courses.DTOs;
using System;
using System.Collections.Generic;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Courses.Queries.GetCourseReviews;

[AllowAnonymous]
public sealed record GetCourseReviewsQuery(Guid CourseId, int PageNumber = 1, int PageSize = 10) : IQuery<PagedResult<CourseReviewDto>>, ICacheableQuery
{
    public string CacheKey => $"course-reviews:{CourseId}:{PageNumber}:{PageSize}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
}
