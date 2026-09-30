using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Application.Features.Sessions.Commands.AddSessionReview;

public sealed class AddSessionReviewCommandHandler : ICommandHandler<AddSessionReviewCommand>
{
    private readonly IApplicationDbContext _context;

    public AddSessionReviewCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(AddSessionReviewCommand request, CancellationToken cancellationToken)
    {
        var session = await _context.FirstOrDefaultAsync(
            _context.Sessions
                .Include(s => s.Reviews)
                .Where(s => s.Id == request.SessionId),
            cancellationToken);

        if (session is null)
        {
            return Result.Failure(new Error("Session.NotFound", "Session was not found.", ErrorType.NotFound));
        }

        // Must have a completed booking
        var hasCompletedBooking = await _context.AnyAsync(
            _context.Bookings.Where(b => b.SessionId == request.SessionId && b.LearnerUserId == request.UserId && b.Status == BookingStatus.Completed),
            cancellationToken);

        if (!hasCompletedBooking)
        {
            return Result.Failure(new Error("Review.BookingRequired", "You must have a completed booking to review this session.", ErrorType.Validation));
        }

        var reviewResult = session.AddReview(request.UserId, request.Stars, request.Comment);
        if (reviewResult.IsFailure)
        {
            return Result.Failure(reviewResult.Errors.First());
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
