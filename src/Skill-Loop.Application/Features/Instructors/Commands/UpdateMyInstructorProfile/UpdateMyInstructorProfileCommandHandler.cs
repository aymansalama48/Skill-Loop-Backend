using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Instructors;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Instructors.Commands.UpdateMyInstructorProfile;

public sealed class UpdateMyInstructorProfileCommandHandler(
    IApplicationDbContext _dbContext) : ICommandHandler<UpdateMyInstructorProfileCommand, bool>
{
    public async Task<Result<bool>> Handle(UpdateMyInstructorProfileCommand request, CancellationToken cancellationToken)
    {
        // استخدام الـ UserId المبعوث في الـ Command
        var profile = await _dbContext.InstructorProfiles
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

        if (profile is null)
        {
            return Result<bool>.Failure(InstructorProfileErrors.NotFound);
        }

        var updateResult = profile.UpdateDetails(request.Headline, request.Bio);
        if (!updateResult.IsSuccess)
        {
            return Result<bool>.Failure(updateResult.Errors);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}