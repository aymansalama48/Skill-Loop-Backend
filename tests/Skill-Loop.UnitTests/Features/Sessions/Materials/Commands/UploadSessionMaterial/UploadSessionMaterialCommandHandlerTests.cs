namespace Skill_Loop.UnitTests.Features.Sessions.Materials.Commands.UploadSessionMaterial;

using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Skill_Loop.Application.Common.Abstractions.Core;
using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.External.Jobs;
using Skill_Loop.Application.Common.Abstractions.External.Storage;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Notifications;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Sessions;
using Skill_Loop.Application.Common.Models.Storage;
using Skill_Loop.Application.Features.Sessions.Materials.Commands.UploadSessionMaterial;
using Skill_Loop.Domain.Common.Results;
// مسار الجلسات الجديد (بصيغة الجمع) ولاحظ اننا شيلنا الـ Session القديم
using Skill_Loop.Domain.Entities.Sessions;
using Skill_Loop.Domain.Enums;
using Skill_Loop.UnitTests.Common;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class UploadSessionMaterialCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly Mock<ICourseContentStorage> _storage;
    private readonly Mock<ICacheService> _cache;
    private readonly Mock<ICurrentUser> _currentUser;
    private readonly Mock<IDateTime> _dateTime;
    private readonly Mock<IJobScheduler> _jobScheduler;
    private readonly UploadSessionMaterialCommandHandler _handler;

    public UploadSessionMaterialCommandHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _storage = new Mock<ICourseContentStorage>();
        _cache = new Mock<ICacheService>();
        _currentUser = new Mock<ICurrentUser>();
        _dateTime = new Mock<IDateTime>();
        _jobScheduler = new Mock<IJobScheduler>();
        _handler = new UploadSessionMaterialCommandHandler(
            _dbContext,
            _storage.Object,
            _cache.Object,
            _currentUser.Object,
            _dateTime.Object,
            _jobScheduler.Object,
            NullLogger<UploadSessionMaterialCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_NoUserId_ReturnsFailure()
    {
        _currentUser.Setup(c => c.UserId).Returns((Guid?)null);

        var result = await _handler.Handle(new UploadSessionMaterialCommand(Guid.NewGuid(), new MemoryStream(), "test.txt", "text/plain", 100), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == SessionMaterialErrors.NotOwner.Code);
    }

    [Fact]
    public async Task Handle_SessionNotFound_ReturnsFailure()
    {
        _currentUser.Setup(c => c.UserId).Returns(Guid.NewGuid());

        var result = await _handler.Handle(new UploadSessionMaterialCommand(Guid.NewGuid(), new MemoryStream(), "test.txt", "text/plain", 100), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == SessionMaterialErrors.SessionNotFound.Code);
    }

    [Fact]
    public async Task Handle_NotOwner_ReturnsFailure()
    {
        var sessionId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        _currentUser.Setup(c => c.UserId).Returns(currentUserId);

        // التعديل هنا: استخدام دالة Create 
        var session = Session.Create(Guid.NewGuid(), Guid.NewGuid(), "Test Session");
        session.Id = sessionId; // تعيين المعرف بعد الإنشاء

        _dbContext.Add(session);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new UploadSessionMaterialCommand(sessionId, new MemoryStream(), "test.txt", "text/plain", 100), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == SessionMaterialErrors.NotOwner.Code);
    }

    [Fact]
    public async Task Handle_UploadSuccess_ReturnsSuccess()
    {
        var sessionId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();

        // التعديل هنا: استخدام دالة Create وإعطاء المحاضر صلاحية الملكية
        var session = Session.Create(currentUserId, currentUserId, "Test Session");
        session.Id = sessionId;

        _dbContext.Add(session);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _currentUser.Setup(c => c.UserId).Returns(currentUserId);
        _storage.Setup(s => s.EnsureSessionFolderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success("folder-1"));
        _storage.Setup(s => s.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CourseContentUploadResult>.Success(new CourseContentUploadResult("file-1", "folder-1", 1024)));
        _storage.Setup(s => s.GetQuotaUsageAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<DriveQuotaUsage>.Success(new DriveQuotaUsage(0, 1000000000)));
        _cache.Setup(c => c.GetAsync<DriveQuotaUsage>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DriveQuotaUsage?)null);
        _dateTime.Setup(d => d.GetDateTimeString()).Returns("2026-01-01T00:00:00Z");

        var result = await _handler.Handle(new UploadSessionMaterialCommand(sessionId, new MemoryStream(), "test.txt", "text/plain", 100, MaterialType.Document), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.FileName.Should().Be("test.txt");
    }
}