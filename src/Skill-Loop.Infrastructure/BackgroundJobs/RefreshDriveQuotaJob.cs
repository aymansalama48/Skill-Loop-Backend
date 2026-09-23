using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Skill_Loop.Application.Common.Abstractions.External.Storage;
using Skill_Loop.Application.Common.Abstractions.Notifications;
using Skill_Loop.Infrastructure.Options;

namespace Skill_Loop.Infrastructure.BackgroundJobs;

public class RefreshDriveQuotaJob
{
    private readonly ICourseContentStorage _storage;
    private readonly IIdentityNotificationService _notificationService;
    private readonly IOptions<GoogleDriveOptions> _options;
    private readonly ILogger<RefreshDriveQuotaJob> _logger;

    public RefreshDriveQuotaJob(
        ICourseContentStorage storage,
        IIdentityNotificationService notificationService,
        IOptions<GoogleDriveOptions> options,
        ILogger<RefreshDriveQuotaJob> logger)
    {
        _storage = storage;
        _notificationService = notificationService;
        _options = options;
        _logger = logger;
    }

    public async Task RefreshAsync()
    {
        try
        {
            var quotaResult = await _storage.GetQuotaUsageAsync(default);
            if (!quotaResult.IsSuccess)
            {
                _logger.LogWarning("Failed to refresh drive quota: {Errors}", quotaResult.Errors);
                return;
            }

            var (usedBytes, totalBytes) = quotaResult.Data;
            var threshold = _options.Value.QuotaWarningThreshold;
            var warningThresholdBytes = (long)(totalBytes * threshold);

            if (usedBytes > warningThresholdBytes)
            {
                await _notificationService.SendStorageQuotaWarningAsync(
                    usedBytes,
                    totalBytes,
                    threshold);
            }

            _logger.LogInformation("Drive quota refreshed: Used={Used}, Total={Total}", usedBytes, totalBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing drive quota");
        }
    }
}