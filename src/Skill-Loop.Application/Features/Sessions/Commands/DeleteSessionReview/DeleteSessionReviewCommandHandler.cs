using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Errors.Auth;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Bookings;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Sessions.Commands.DeleteSessionReview;

internal sealed class DeleteSessionReviewCommandHandler(IApplicationDbContext _dbContext, ICurrentUser _currentUser) : ICommandHandler<DeleteSessionReviewCommand>
{
    public async Task<Result> Handle(DeleteSessionReviewCommand request, CancellationToken cancellationToken)
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

        var result = session.RemoveReview(request.UserId);

        if (result.IsFailure)
        {
            return result;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
