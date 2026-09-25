using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Features.Instructors.Share;

namespace Skill_Loop.Application.Features.Instructors.Queries.GetInstructorProfileByUserId;

public sealed record GetInstructorProfileByUserIdQuery(
    Guid UserId
) : ICacheableQuery<InstructorProfileResponse>
{
    public string CacheKey => $"instructor-profile-userid-{UserId}";
    public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(10);
    public TimeSpan? AbsoluteExpiration => TimeSpan.FromHours(1);
}