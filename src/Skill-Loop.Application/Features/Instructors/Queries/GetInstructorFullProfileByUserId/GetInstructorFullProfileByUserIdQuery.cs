using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Features.Instructors.Share;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Instructors.Queries.GetInstructorFullProfileByUserId;

[AuthenticatedOnly]
public sealed record GetInstructorFullProfileByUserIdQuery(
    Guid UserId
) : ICacheableQuery<InstructorDetailsResponse>
{
    public string CacheKey => $"instructor-full-profile-userid-{UserId}";
    public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(10);
    public TimeSpan? AbsoluteExpiration => TimeSpan.FromHours(1);
}