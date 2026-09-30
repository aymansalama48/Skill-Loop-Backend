using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Auth;
using Skill_Loop.Application.Common.Errors.Identity;
using Skill_Loop.Application.Common.Errors.Instructors;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Constants;
using Skill_Loop.Domain.Entities.Instructors;

namespace Skill_Loop.Application.Features.Instructors.Commands.CreateMyInstructorProfile;

public sealed class CreateMyInstructorProfileCommandHandler(
    IApplicationDbContext _dbContext,
    ICurrentUser _currentUser) : ICommandHandler<CreateMyInstructorProfileCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateMyInstructorProfileCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue || _currentUser.UserId.Value == Guid.Empty)
        {
            return Result<Guid>.Failure(UserErrors.NotFound);
        }

        // Only an account that actually holds the Instructor role may self-register a
        // profile. Without this check any authenticated user could mint themselves an
        // instructor profile, because the command was only marked [AuthenticatedOnly].
        if (!_currentUser.IsInRole(Roles.Instructor) && !_currentUser.IsInRole(Roles.Admin)
            && !_currentUser.IsInRole(Roles.SuperAdmin))
        {
            return Result<Guid>.Failure(AuthErrors.Forbidden);
        }

        var userId = _currentUser.UserId.Value;

        var exists = await _dbContext.InstructorProfiles
            .AnyAsync(p => p.UserId == userId, cancellationToken);

        if (exists)
        {
            return Result<Guid>.Failure(InstructorProfileErrors.AlreadyExists);
        }

        var profileResult = InstructorProfile.Create(userId, request.Headline, request.Bio);
        if (!profileResult.IsSuccess)
        {
            return Result<Guid>.Failure(profileResult.Errors);
        }

        var profile = profileResult.Data;
        _dbContext.Add(profile);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(profile.Id);
    }
}