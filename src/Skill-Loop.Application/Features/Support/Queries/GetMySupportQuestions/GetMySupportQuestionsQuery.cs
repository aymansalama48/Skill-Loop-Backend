using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Support.Share;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Support.Queries.GetMySupportQuestions;

/// <summary>
/// الاستفسارات اللي المستخدم نفسه أرسلها — بيشوف حالة كل واحد والرد عليه.
/// المستخدم بيتقرأ من الـ JWT، مش من الـ request.
/// </summary>
[AuthenticatedOnly]
public sealed record GetMySupportQuestionsQuery(
    Guid UserId,
    PaginationParameters Pagination) : ICacheableQuery<PagedResult<MySupportQuestionResponse>>
{
    public string CacheKey => $"support:my-questions:{UserId}:page:{Pagination?.PageNumber}:size:{Pagination?.PageSize}";

    public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(10);
    public TimeSpan? AbsoluteExpiration => TimeSpan.FromHours(1);
}
