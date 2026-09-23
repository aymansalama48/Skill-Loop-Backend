using Skill_Loop.Application.Common.Abstractions.Core;
using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.External.Jobs;
using Skill_Loop.Application.Common.Abstractions.External.Storage;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Notifications;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Constants;
using Skill_Loop.Application.Common.Errors.Sessions;
using Skill_Loop.Application.Common.Helpers;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Session;
using Skill_Loop.Domain.Entities.SessionMaterial;
using Skill_Loop.Domain.Entities.SessionMaterial.Events;
using Skill_Loop.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Skill_Loop.Application.Common.Models.Storage;

namespace Skill_Loop.Application.Features.Sessions.Materials.Commands.UploadSessionMaterial;

public sealed class UploadSessionMaterialCommandHandler : ICommandHandler<UploadSessionMaterialCommand, UploadSessionMaterialResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICourseContentStorage _courseContentStorage;
    private readonly ICacheService _cacheService;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTime _dateTime;
    private readonly IJobScheduler _jobScheduler;
    private readonly ILogger<UploadSessionMaterialCommandHandler> _logger;

    private const string QuotaCacheKey = "drive-quota-usage";
    private const long SafetyMarginBytes = 100 * 1024 * 1024; // 100 MB safety margin
    private const int MaxMaterialsPerSession = 50;

    public UploadSessionMaterialCommandHandler(
        IApplicationDbContext dbContext,
        ICourseContentStorage courseContentStorage,
        ICacheService cacheService,
        ICurrentUser currentUser,
        IDateTime dateTime,
        IJobScheduler jobScheduler,
        ILogger<UploadSessionMaterialCommandHandler> logger)
    {
        _dbContext = dbContext;
        _courseContentStorage = courseContentStorage;
        _cacheService = cacheService;
        _currentUser = currentUser;
        _dateTime = dateTime;
        _jobScheduler = jobScheduler;
        _logger = logger;
    }

    public async Task<Result<UploadSessionMaterialResponse>> Handle(
        UploadSessionMaterialCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Validate current user
        if (!_currentUser.UserId.HasValue || _currentUser.UserId.Value == Guid.Empty)
        {
            return Result<UploadSessionMaterialResponse>.Failure(SessionMaterialErrors.NotOwner);
        }

        var currentUserId = _currentUser.UserId.Value;

        // 2. Get session and verify ownership + status
        var session = await _dbContext.Sessions
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

        if (session is null)
        {
            return Result<UploadSessionMaterialResponse>.Failure(SessionMaterialErrors.SessionNotFound);
        }

        // Only owner can upload (InstructorId or OwnerId)
        if (session.InstructorId != currentUserId && session.OwnerId != currentUserId)
        {
            return Result<UploadSessionMaterialResponse>.Failure(SessionMaterialErrors.NotOwner);
        }

        // Only Draft or Published sessions can have materials
        if (session.Status != SessionStatus.Draft && session.Status != SessionStatus.Published)
        {
            return Result<UploadSessionMaterialResponse>.Failure(SessionMaterialErrors.SessionNotAvailable);
        }

        // 3. Check material limit per session
        var currentMaterialCount = await _dbContext.SessionMaterials
            .CountAsync(m => m.SessionId == request.SessionId, cancellationToken);

        if (currentMaterialCount >= MaxMaterialsPerSession)
        {
            return Result<UploadSessionMaterialResponse>.Failure(SessionMaterialErrors.MaterialLimitReachedForSession);
        }

        // 4. Check quota (cached + safety margin)
        var quotaResult = await GetQuotaWithSafetyMarginAsync(cancellationToken);
        if (!quotaResult.IsSuccess)
        {
            return Result<UploadSessionMaterialResponse>.Failure(quotaResult.Errors);
        }

        var (usedBytes, totalBytes) = quotaResult.Data;
        var projectedUsed = usedBytes + request.SizeBytes + SafetyMarginBytes;

        if (projectedUsed > totalBytes)
        {
            _logger.LogWarning("Quota exceeded for user {UserId}. Used: {Used}, Projected: {Projected}, Total: {Total}",
                currentUserId, usedBytes, projectedUsed, totalBytes);
            return Result<UploadSessionMaterialResponse>.Failure(SessionMaterialErrors.StorageQuotaExceeded);
        }

        // 5. Ensure session folder exists
        var folderResult = await _courseContentStorage.EnsureSessionFolderAsync(request.SessionId, cancellationToken);
        if (!folderResult.IsSuccess)
        {
            _logger.LogError("Failed to ensure session folder for session {SessionId}: {Errors}",
                request.SessionId, folderResult.Errors);
            return Result<UploadSessionMaterialResponse>.Failure(SessionMaterialErrors.UploadFailed);
        }

        var folderId = folderResult.Data;

        // 6. Upload file to storage
        var uploadResult = await _courseContentStorage.UploadAsync(
            request.FileStream,
            request.FileName,
            folderId,
            cancellationToken);

        if (!uploadResult.IsSuccess)
        {
            _logger.LogError("Failed to upload file for session {SessionId}: {Errors}",
                request.SessionId, uploadResult.Errors);
            return Result<UploadSessionMaterialResponse>.Failure(SessionMaterialErrors.UploadFailed);
        }

        var uploadData = uploadResult.Data;

        // 7. Determine sort order (append at end)
        var maxSortOrder = await _dbContext.SessionMaterials
            .Where(m => m.SessionId == request.SessionId)
            .MaxAsync(m => (int?)m.SortOrder, cancellationToken) ?? -1;

        var sortOrder = maxSortOrder + 1;

        // 8. Create DB row via factory
        var material = SessionMaterial.Create(
            request.SessionId,
            request.FileName,
            request.MimeType,
            request.SizeBytes,
            uploadData.DriveFileId,
            uploadData.DriveFolderId,
            sortOrder,
            currentUserId,
            request.MaterialType);

        _dbContext.Add(material);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Uploaded material {MaterialId} for session {SessionId} by user {UserId}",
            material.Id, request.SessionId, currentUserId);

        // 9. Return response
        var response = new UploadSessionMaterialResponse(
            material.Id,
            material.SessionId,
            material.FileName,
            material.MimeType,
            material.SizeBytes,
            material.DriveFileId,
            material.DriveFolderId,
            material.SortOrder,
            material.MaterialType,
            material.CreatedAt);

        return Result<UploadSessionMaterialResponse>.Success(response);
    }

    private async Task<Result<(long UsedBytes, long TotalBytes)>> GetQuotaWithSafetyMarginAsync(CancellationToken cancellationToken)
    {
        var cached = await _cacheService.GetAsync<DriveQuotaUsage>(QuotaCacheKey, cancellationToken);

        if (cached is not null)
        {
            return Result<(long, long)>.Success((cached.UsedBytes, cached.TotalBytes));
        }

        var quotaResult = await _courseContentStorage.GetQuotaUsageAsync(cancellationToken);
        if (!quotaResult.IsSuccess)
        {
            return Result<(long, long)>.Failure(quotaResult.Errors);
        }

        var quota = quotaResult.Data;
        await _cacheService.SetAsync(QuotaCacheKey, quota, TimeSpan.FromMinutes(10), TimeSpan.FromHours(1), cancellationToken);

        return Result<(long, long)>.Success((quota.UsedBytes, quota.TotalBytes));
    }
}