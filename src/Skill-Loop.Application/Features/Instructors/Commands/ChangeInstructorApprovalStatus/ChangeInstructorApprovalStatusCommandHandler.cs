using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Instructors;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Instructors.Commands.ChangeInstructorApprovalStatus;

public sealed class ChangeInstructorApprovalStatusCommandHandler(
    IApplicationDbContext _dbContext) : ICommandHandler<ChangeInstructorApprovalStatusCommand, bool>
{
    public async Task<Result<bool>> Handle(ChangeInstructorApprovalStatusCommand request, CancellationToken cancellationToken)
    {
        var profile = await _dbContext.InstructorProfiles
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

        if (profile is null)
        {
            return Result<bool>.Failure(InstructorProfileErrors.NotFound);
        }

        // استخدام دوال الـ Domain بناءً على الطلب
        if (request.IsApproved)
        {
            profile.Approve();
        }
        else
        {
            profile.Suspend();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}