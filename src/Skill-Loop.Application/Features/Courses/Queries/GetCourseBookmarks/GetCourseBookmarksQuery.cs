using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Courses.DTOs;
using System;
using System.Collections.Generic;

namespace Skill_Loop.Application.Features.Courses.Queries.GetCourseBookmarks;

public sealed record GetCourseBookmarksQuery(int PageNumber = 1, int PageSize = 10) : IQuery<PagedResult<CourseSummaryDto>>;
