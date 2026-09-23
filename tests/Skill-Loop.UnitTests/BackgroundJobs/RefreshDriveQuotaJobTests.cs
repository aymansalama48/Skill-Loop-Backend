namespace Skill_Loop.UnitTests.BackgroundJobs;

using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Skill_Loop.Application.Common.Abstractions.External.Storage;
using Skill_Loop.Application.Common.Abstractions.Notifications;
using Skill_Loop.Application.Common.Models.Storage;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Infrastructure.BackgroundJobs;
using Skill_Loop.Infrastructure.Options;
using Xunit;

public class RefreshDriveQuotaJobTests
{
    [Fact]
    public async Task RefreshAsync_QuotaExceedsThreshold_SendsWarning()
    {
        var storage = new Mock<ICourseContentStorage>();
        var notificationService = new Mock<IIdentityNotificationService>();
        var options = new Mock<Microsoft.Extensions.Options.IOptions<GoogleDriveOptions>>();

        storage.Setup(s => s.GetQuotaUsageAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<DriveQuotaUsage>.Success(new DriveQuotaUsage(900_000_000_000, 1_000_000_000_000)));
        options.Setup(o => o.Value).Returns(new GoogleDriveOptions
        {
            ServiceAccountFilePath = "test.json",
            RootFolderId = "root",
            QuotaWarningThreshold = 0.8,
        });

        var job = new RefreshDriveQuotaJob(
            storage.Object,
            notificationService.Object,
            options.Object,
            NullLogger<RefreshDriveQuotaJob>.Instance);

        await job.RefreshAsync();

        notificationService.Verify(n => n.SendStorageQuotaWarningAsync(
            It.IsAny<long>(), It.IsAny<long>(), It.IsAny<double>()), Times.Once);
    }

    [Fact]
    public async Task RefreshAsync_QuotaBelowThreshold_DoesNotSendWarning()
    {
        var storage = new Mock<ICourseContentStorage>();
        var notificationService = new Mock<IIdentityNotificationService>();
        var options = new Mock<Microsoft.Extensions.Options.IOptions<GoogleDriveOptions>>();

        storage.Setup(s => s.GetQuotaUsageAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<DriveQuotaUsage>.Success(new DriveQuotaUsage(500_000_000_000, 1_000_000_000_000)));
        options.Setup(o => o.Value).Returns(new GoogleDriveOptions
        {
            ServiceAccountFilePath = "test.json",
            RootFolderId = "root",
            QuotaWarningThreshold = 0.8,
        });

        var job = new RefreshDriveQuotaJob(
            storage.Object,
            notificationService.Object,
            options.Object,
            NullLogger<RefreshDriveQuotaJob>.Instance);

        await job.RefreshAsync();

        notificationService.Verify(n => n.SendStorageQuotaWarningAsync(
            It.IsAny<long>(), It.IsAny<long>(), It.IsAny<double>()), Times.Never);
    }

    [Fact]
    public async Task RefreshAsync_StorageFailure_DoesNotThrow()
    {
        var storage = new Mock<ICourseContentStorage>();
        var notificationService = new Mock<IIdentityNotificationService>();
        var options = new Mock<Microsoft.Extensions.Options.IOptions<GoogleDriveOptions>>();

        storage.Setup(s => s.GetQuotaUsageAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<DriveQuotaUsage>.Failure("Storage error"));

        var job = new RefreshDriveQuotaJob(
            storage.Object,
            notificationService.Object,
            options.Object,
            NullLogger<RefreshDriveQuotaJob>.Instance);

        await job.RefreshAsync();

        notificationService.Verify(n => n.SendStorageQuotaWarningAsync(
            It.IsAny<long>(), It.IsAny<long>(), It.IsAny<double>()), Times.Never);
    }
}
