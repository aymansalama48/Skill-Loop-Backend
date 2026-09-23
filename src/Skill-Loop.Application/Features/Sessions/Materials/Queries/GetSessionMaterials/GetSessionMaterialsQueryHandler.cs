using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Sessions;
using Skill_Loop.Application.Common.Helpers;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Booking;
using Skill_Loop.Domain.Entities.Session;
using Skill_Loop.Domain.Entities.SessionMaterial;
using Skill_Loop.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Skill_Loop.Application.Features.Sessions.Materials.Queries.GetSessionMaterials;

public sealed class GetSessionMaterialsQueryHandler : IQueryHandler<GetSessionMaterialsQuery, PagedResult<SessionMaterialResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly IPermissionService _permissionService;

    public GetSessionMaterialsQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        IPermissionService permissionService)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _permissionService = permissionService;
    }

    public async Task<Result<PagedResult<SessionMaterialResponse>>> Handle(
        GetSessionMaterialsQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Validate current user
        if (!_currentUser.UserId.HasValue || _currentUser.UserId.Value == Guid.Empty)
        {
            return Result<PagedResult<SessionMaterialResponse>>.Failure(SessionMaterialErrors.NotAuthorizedToView);
        }

        var currentUserId = _currentUser.UserId.Value;

        // 2. Get session
        var session = await _dbContext.Sessions
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

        if (session is null)
        {
            return Result<PagedResult<SessionMaterialResponse>>.Failure(SessionMaterialErrors.SessionNotFound);
        }

        // 3. Check visibility permissions
        var isOwner = session.InstructorId == currentUserId || session.OwnerId == currentUserId;
        var isAdmin = await _permissionService.HasPermissionAsync(currentUserId, "Sessions.Moderate", cancellationToken);

        var hasActiveBooking = false;
        if (!isOwner && !isAdmin)
        {
            hasActiveBooking = await _dbContext.Bookings
                .AnyAsync(b =>
                    b.SessionId == request.SessionId &&
                    b.LearnerUserId == currentUserId &&
                    (b.Status == BookingStatus.Confirmed ||
                     b.Status == BookingStatus.InProgress ||
                     b.Status == BookingStatus.Completed),
                    cancellationToken);
        }

        if (!isOwner && !isAdmin && !hasActiveBooking)
        {
            return Result<PagedResult<SessionMaterialResponse>>.Failure(SessionMaterialErrors.NotAuthorizedToView);
        }

        // 4. Query materials with pagination (AsNoTracking)
        var query = _dbContext.SessionMaterials
            .AsNoTracking()
            .Where(m => m.SessionId == request.SessionId)
            .OrderBy(m => m.SortOrder)
            .ThenBy(m => m.CreatedAt);

        var totalCount = await _dbContext.CountAsync(query, cancellationToken);

        var items = await _dbContext.ToListAsync(
            query.Skip(request.Pagination.Skip).Take(request.Pagination.PageSize),
            cancellationToken);

        var responses = items.Select(m => new SessionMaterialResponse(
            m.Id,
            m.SessionId,
            m.FileName,
            m.MimeType,
            m.SizeBytes,
            m.DriveFileId,
            m.DriveFolderId,
            m.SortOrder,
            m.MaterialType,
            m.CreatedAt)).ToList();

        var paginationMetadata = new PaginationMetadata
        {
            CurrentPage = request.Pagination.PageNumber,
            PageSize = request.Pagination.PageSize,
            TotalCount = totalCount
        };

        var pagedResult = new PagedResult<SessionMaterialResponse>
        {
            Items = responses,
            Pagination = paginationMetadata
        };

        return Result<PagedResult<SessionMaterialResponse>>.Success(pagedResult);
    }
}