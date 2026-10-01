using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Abstractions.Identity.UserManagement;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Bookings;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Bookings.Shared;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Bookings.Queries.GetSessionBookings;

public sealed class GetSessionBookingsQueryHandler
    : IQueryHandler<GetSessionBookingsQuery, PagedResult<BookingResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly IUserManagementService _userService;

    public GetSessionBookingsQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        IUserManagementService userService)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _userService = userService;
    }

    public async Task<Result<PagedResult<BookingResponse>>> Handle(
        GetSessionBookingsQuery request,
        CancellationToken cancellationToken)
    {
        var session = await _dbContext.Sessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

        if (session is null)
        {
            return Result<PagedResult<BookingResponse>>.Failure(BookingErrors.SessionNotFound);
        }

        // A session's roster is only visible to its own instructor/owner or a global booking manager.
        var currentUserId = _currentUser.UserId;
        var isInstructor = currentUserId.HasValue &&
                           (session.InstructorId == currentUserId.Value || session.OwnerId == currentUserId.Value);
        var canManageAll = _currentUser.HasPermission(Permissions.Bookings.ViewAll)
                           || _currentUser.HasPermission(Permissions.Bookings.ManageAll);

        if (!isInstructor && !canManageAll)
        {
            return Result<PagedResult<BookingResponse>>.Failure(BookingErrors.NotLearner);
        }

        var query = _dbContext.Bookings
            .AsNoTracking()
            .Where(b => b.SessionId == request.SessionId);

        if (request.Status.HasValue)
        {
            var status = request.Status.Value;
            query = query.Where(b => b.Status == status);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var pagedBookings = await query
            .OrderByDescending(b => b.BookedAtUtc)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var items = await BookingResponseFactory.CreateListAsync(_dbContext, _userService, pagedBookings, cancellationToken);

        return Result<PagedResult<BookingResponse>>.Success(new PagedResult<BookingResponse>
        {
            Items = items,
            Pagination = new PaginationMetadata
            {
                CurrentPage = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount
            }
        });
    }
}
