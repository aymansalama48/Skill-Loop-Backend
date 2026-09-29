using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Features.Instructors.Share;

namespace Skill_Loop.Application.Features.Instructors.Queries.GetInstructorFullProfileByUserId;

public sealed record GetInstructorFullProfileByUserIdQuery(
    Guid UserId
) : ICacheableQuery<InstructorDetailsResponse>
{
    public string CacheKey => $"instructor-full-profile-userid-{UserId}";
    public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(10);
    public TimeSpan? AbsoluteExpiration => TimeSpan.FromHours(1);
}