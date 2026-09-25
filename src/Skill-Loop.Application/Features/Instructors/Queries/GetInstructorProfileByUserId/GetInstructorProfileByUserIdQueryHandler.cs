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
        var profile = await _dbContext.InstructorProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

        if (profile is null)
        {
            return Result<InstructorProfileResponse>.Failure(InstructorProfileErrors.NotFound);
        }

        var dto = new InstructorProfileResponse(
            profile.Id,
            profile.UserId,
            profile.Headline,
            profile.Bio,
            profile.IsApproved,
            profile.Rating,
            profile.SessionsCompleted,
            profile.CreditsEarned
        );

        return Result<InstructorProfileResponse>.Success(dto);
    }
}