using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Errors.Auth;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Bookings; // Or SessionErrors
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Sessions.Commands.UpdateSessionReview;

internal sealed class UpdateSessionReviewCommandHandler(IApplicationDbContext _dbContext, ICurrentUser _currentUser) : ICommandHandler<UpdateSessionReviewCommand>
{
    public async Task<Result> Handle(UpdateSessionReviewCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId != request.UserId)
        {
            return Result.Failure(AuthErrors.Forbidden);
        }
        var session = await _dbContext.Sessions
            .Include(s => s.Reviews)
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

        if (session is null)
        {
            return Result.Failure(BookingErrors.SessionNotFound);
        }

        var result = session.UpdateReview(request.UserId, request.Stars, request.Comment);

        if (result.IsFailure)
        {
            return result;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
