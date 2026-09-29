using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Instructors;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Instructors.Commands.AddInstructorAvailability;

public sealed class AddInstructorAvailabilityCommandHandler(
    IApplicationDbContext _dbContext) : ICommandHandler<AddInstructorAvailabilityCommand, bool>
{
    public async Task<Result<bool>> Handle(AddInstructorAvailabilityCommand request, CancellationToken cancellationToken)
    {
        // بنجيب البروفايل مع مواعيده الحالية عشان نتشيك لو فيه تداخل
        var profile = await _dbContext.InstructorProfiles
            .Include(p => p.Availabilities)
            .FirstOrDefaultAsync(p => p.Id == request.InstructorProfileId, cancellationToken);

        if (profile is null)
        {
            return Result<bool>.Failure(InstructorProfileErrors.NotFound);
        }

        // استدعاء دالة الدومين اللي بنيناها
        var addResult = profile.AddAvailability(request.DayOfWeek, request.StartTime, request.EndTime);

        if (!addResult.IsSuccess)
        {
            return Result<bool>.Failure(addResult.Errors);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}