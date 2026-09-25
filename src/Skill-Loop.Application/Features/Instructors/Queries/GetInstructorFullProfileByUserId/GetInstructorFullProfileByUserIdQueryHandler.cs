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
        // 1. جلب بيانات البروفايل من Application DB
        var profile = await _dbContext.InstructorProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

        if (profile is null)
        {
            return Result<InstructorDetailsResponse>.Failure(InstructorProfileErrors.NotFound);
        }

        // 2. جلب بيانات المستخدم من Identity Service
        var userResult = await _userService.GetByIdAsync(request.UserId, cancellationToken);

        if (!userResult.IsSuccess || userResult.Data is null)
        {
            return Result<InstructorDetailsResponse>.Failure(InstructorProfileErrors.NotFound);
        }

        var user = userResult.Data;

        // 3. دمج البيانات في DTO واحد يجمع كل التفاصيل
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
            profile.CreditsEarned
        );

        return Result<InstructorDetailsResponse>.Success(dto);
    }
}