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
using Skill_Loop.Domain.Entities.Sessions; // مسار الجلسة
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

    private const string QuotaCacheKey = "drive-quota-usage-v3";
    private const long SafetyMarginBytes = 100 * 1024 * 1024;
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

        // 2. Get session (نجلب الجلسة مع ملفاتها الحالية عشان نقدر نضيف فيها ونحسب عددها)
        var session = await _dbContext.Sessions
            .Include(s => s.Materials)
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

        if (session is null)
        {
            return Result<UploadSessionMaterialResponse>.Failure(SessionMaterialErrors.SessionNotFound);
        }

        if (session.InstructorId != currentUserId && session.OwnerId != currentUserId)
        {
            return Result<UploadSessionMaterialResponse>.Failure(SessionMaterialErrors.NotOwner);
        }

        if (session.Status != SessionStatus.Draft && session.Status != SessionStatus.Published)
        {
            return Result<UploadSessionMaterialResponse>.Failure(SessionMaterialErrors.SessionNotAvailable);
        }

        // 3. Check material limit per session (استخدمنا الـ Entity مباشرة)
        if (session.Materials.Count >= MaxMaterialsPerSession)
        {
            return Result<UploadSessionMaterialResponse>.Failure(SessionMaterialErrors.MaterialLimitReachedForSession);
        }

        // 4. Check quota
        var quotaResult = await GetQuotaWithSafetyMarginAsync(cancellationToken);
        if (!quotaResult.IsSuccess) return Result<UploadSessionMaterialResponse>.Failure(quotaResult.Errors);

        var (usedBytes, totalBytes) = quotaResult.Data;
        var projectedUsed = usedBytes + request.SizeBytes + SafetyMarginBytes;

        if (projectedUsed > totalBytes)
        {
            _logger.LogWarning("Quota exceeded...");
            return Result<UploadSessionMaterialResponse>.Failure(SessionMaterialErrors.StorageQuotaExceeded);
        }

        // 5. Ensure session folder
        var folderResult = await _courseContentStorage.EnsureSessionFolderAsync(request.SessionId, cancellationToken);
        if (!folderResult.IsSuccess) return Result<UploadSessionMaterialResponse>.Failure(folderResult.Errors);

        // 6. Upload file
        var uploadResult = await _courseContentStorage.UploadAsync(request.FileStream, request.FileName, folderResult.Data, cancellationToken);
        if (!uploadResult.IsSuccess) return Result<UploadSessionMaterialResponse>.Failure(uploadResult.Errors);

        var uploadData = uploadResult.Data;

        // 7. Determine sort order
        var sortOrder = session.Materials.Any() ? session.Materials.Max(m => m.SortOrder) + 1 : 0;

        // 8. Create & Add to Session (هنا التغيير! استخدمنا الدالة بتاعتك)
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

        // إضافة الملف للجلسة عن طريق الدومين
        session.AddMaterial(material);

        // مش محتاجين نعمل Add للـ _dbContext.SessionMaterials لأننا ضفناه في الأب (Session)
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Uploaded material {MaterialId} for session {SessionId} by user {UserId}", material.Id, request.SessionId, currentUserId);

        var response = new UploadSessionMaterialResponse(
            material.Id, material.SessionId, material.FileName, material.MimeType,
            material.SizeBytes, material.DriveFileId, material.DriveFolderId,
            material.SortOrder, material.MaterialType, material.CreatedAt);

        return Result<UploadSessionMaterialResponse>.Success(response);
    }

    private async Task<Result<(long UsedBytes, long TotalBytes)>> GetQuotaWithSafetyMarginAsync(CancellationToken cancellationToken)
    {
        var cached = await _cacheService.GetAsync<DriveQuotaUsage>(QuotaCacheKey, cancellationToken);
        if (cached is not null) return Result<(long, long)>.Success((cached.UsedBytes, cached.TotalBytes));

        var quotaResult = await _courseContentStorage.GetQuotaUsageAsync(cancellationToken);
        if (!quotaResult.IsSuccess) return Result<(long, long)>.Failure(quotaResult.Errors);

        var quota = quotaResult.Data;
        await _cacheService.SetAsync(QuotaCacheKey, quota, TimeSpan.FromMinutes(10), TimeSpan.FromHours(1), cancellationToken);

        return Result<(long, long)>.Success((quota.UsedBytes, quota.TotalBytes));
    }
}