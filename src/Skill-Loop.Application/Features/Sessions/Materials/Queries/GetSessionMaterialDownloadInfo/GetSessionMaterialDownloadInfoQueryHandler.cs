using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Sessions;
using Skill_Loop.Application.Common.Helpers;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Booking;
using Skill_Loop.Domain.Entities.Session;
using Skill_Loop.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Skill_Loop.Application.Features.Sessions.Materials.Queries.GetSessionMaterialDownloadInfo;

public sealed class GetSessionMaterialDownloadInfoQueryHandler : IQueryHandler<GetSessionMaterialDownloadInfoQuery, SessionMaterialDownloadInfoResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly IPermissionService _permissionService;

    public GetSessionMaterialDownloadInfoQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        IPermissionService permissionService)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _permissionService = permissionService;
    }

    public async Task<Result<SessionMaterialDownloadInfoResponse>> Handle(
        GetSessionMaterialDownloadInfoQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Validate current user
        if (!_currentUser.UserId.HasValue || _currentUser.UserId.Value == Guid.Empty)
        {
            return Result<SessionMaterialDownloadInfoResponse>.Failure(SessionMaterialErrors.NotAuthorizedToView);
        }

        var currentUserId = _currentUser.UserId.Value;

        // 2. Get material with session
        var material = await _dbContext.SessionMaterials
            .AsNoTracking()
            .Include(m => m.Session)
            .FirstOrDefaultAsync(m => m.Id == request.MaterialId && m.SessionId == request.SessionId, cancellationToken);

        if (material is null)
        {
            return Result<SessionMaterialDownloadInfoResponse>.Failure(SessionMaterialErrors.NotFound);
        }

        // 3. Check visibility permissions (same as GetSessionMaterialsQuery)
        var isOwner = material.Session.InstructorId == currentUserId || material.Session.OwnerId == currentUserId;
        var isAdmin = await _permissionService.HasPermissionAsync(currentUserId, "Sessions.Moderate", cancellationToken);

        var hasActiveBooking = false;
        if (!isOwner && !isAdmin)
        {
            hasActiveBooking = await _dbContext.Bookings
                .AnyAsync(b =>
                    b.SessionId == request.SessionId &&
                    b.LearnerUserId == currentUserId &&
                    (b.Status == BookingStatus.Confirmed ||
                     b.Status == BookingStatus.Completed), // اكتفينا بالحالتين دول فقط
                    cancellationToken);
        }

        if (!isOwner && !isAdmin && !hasActiveBooking)
        {
            return Result<SessionMaterialDownloadInfoResponse>.Failure(SessionMaterialErrors.NotAuthorizedToView);
        }

        // 4. Return metadata only (no stream)
        var response = new SessionMaterialDownloadInfoResponse(
            material.Id,
            material.FileName,
            material.MimeType,
            material.SizeBytes,
            material.DriveFileId);

        return Result<SessionMaterialDownloadInfoResponse>.Success(response);
    }
}