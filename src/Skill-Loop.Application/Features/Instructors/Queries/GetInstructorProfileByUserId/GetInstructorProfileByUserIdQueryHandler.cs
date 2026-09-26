using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Instructors;
using Skill_Loop.Application.Features.Instructors.Share;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Instructors.Queries.GetInstructorProfileByUserId;

public sealed class GetInstructorProfileByUserIdQueryHandler(
    IApplicationDbContext _dbContext) : IQueryHandler<GetInstructorProfileByUserIdQuery, InstructorProfileResponse>
{
    public async Task<Result<InstructorProfileResponse>> Handle(GetInstructorProfileByUserIdQuery request, CancellationToken cancellationToken)
    {
        // 1. جلب البروفايل مع المواعيد والتقييمات
        var profile = await _dbContext.InstructorProfiles
            .Include(p => p.Availabilities)
            .Include(p => p.Reviews)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

        if (profile is null)
        {
            return Result<InstructorProfileResponse>.Failure(InstructorProfileErrors.NotFound);
        }

        // 2. تحويل (Mapping) المواعيد
        var availabilitiesList = profile.Availabilities.Select(a => new InstructorAvailabilityResponse(
            a.Id,
            a.DayOfWeek.ToString(),
            a.StartTime,
            a.EndTime
        )).ToList().AsReadOnly();

        // 3. تحويل (Mapping) التقييمات
        var reviewsList = profile.Reviews
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new InstructorReviewResponse(
                r.Id,
                r.LearnerUserId,
                r.Rating,
                r.Comment,
                r.CreatedAt
            )).ToList().AsReadOnly();

        // 4. تجميع الـ DTO النهائي
        var dto = new InstructorProfileResponse(
            profile.Id,
            profile.UserId,
            profile.Headline,
            profile.Bio,
            profile.IsApproved,
            profile.Rating,
            profile.SessionsCompleted,
            profile.CreditsEarned,
            availabilitiesList,
            reviewsList
        );

        return Result<InstructorProfileResponse>.Success(dto);
    }
}