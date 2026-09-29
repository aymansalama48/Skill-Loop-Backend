using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Identity.UserManagement;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Instructors;
using Skill_Loop.Application.Features.Instructors.Share;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Instructors.Queries.GetInstructorFullProfileByUserId;

public sealed class GetInstructorFullProfileByUserIdQueryHandler(
    IApplicationDbContext _dbContext,
    IUserManagementService _userService) : IQueryHandler<GetInstructorFullProfileByUserIdQuery, InstructorDetailsResponse>
{
    public async Task<Result<InstructorDetailsResponse>> Handle(GetInstructorFullProfileByUserIdQuery request, CancellationToken cancellationToken)
    {
        // 1. جلب بيانات البروفايل مع المواعيد والتقييمات في كويري واحد
        var profile = await _dbContext.InstructorProfiles
            .Include(p => p.Availabilities)
            .Include(p => p.Reviews)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

        if (profile is null)
        {
            return Result<InstructorDetailsResponse>.Failure(InstructorProfileErrors.NotFound);
        }

        // 2. جلب بيانات المستخدم الأساسية من الـ Identity
        var userResult = await _userService.GetByIdAsync(request.UserId, cancellationToken);

        if (!userResult.IsSuccess || userResult.Data is null)
        {
            return Result<InstructorDetailsResponse>.Failure(InstructorProfileErrors.NotFound);
        }

        var user = userResult.Data;

        // 3. تحويل (Mapping) المواعيد
        var availabilitiesList = profile.Availabilities.Select(a => new InstructorAvailabilityResponse(
            a.Id,
            a.DayOfWeek.ToString(),
            a.StartTime,
            a.EndTime
        )).ToList().AsReadOnly();

        // 4. تحويل (Mapping) التقييمات (بنرتبهم من الأحدث للأقدم)
        var reviewsList = profile.Reviews
            .OrderByDescending(r => r.CreatedAt) // بنفترض إنك بتستخدم CreatedAt من الـ AuditableEntity
            .Select(r => new InstructorReviewResponse(
                r.Id,
                r.LearnerUserId,
                r.Rating,
                r.Comment,
                r.CreatedAt // تحويل من DateTimeOffset لـ DateTime لو لزم الأمر، أو استخدامها مباشرة
            )).ToList().AsReadOnly();

        // 5. دمج كل شيء في الـ Response النهائي
        var dto = new InstructorDetailsResponse(
            profile.Id,
            profile.UserId,
            user.FirstName,
            user.LastName,
            user.FullName,
            user.AvatarUrl,
            profile.Headline,
            profile.Bio,
            profile.IsApproved,
            profile.Rating,
            profile.SessionsCompleted,
            profile.CreditsEarned,
            availabilitiesList,
            reviewsList
        );

        return Result<InstructorDetailsResponse>.Success(dto);
    }
}