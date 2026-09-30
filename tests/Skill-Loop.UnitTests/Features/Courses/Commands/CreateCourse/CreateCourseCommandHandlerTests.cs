using FluentAssertions;
using Moq;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Abstractions.External.FileStorage;
using Skill_Loop.Application.Common.Errors.Category;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Features.Courses.Commands.CreateCourse;
using Skill_Loop.Domain.Constants;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Enums;
using Skill_Loop.UnitTests.Common;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Skill_Loop.Domain.Entities.Courses;

namespace Skill_Loop.UnitTests.Features.Courses.Commands.CreateCourse;

public class CreateCourseCommandHandlerTests : IDisposable
{
    private readonly IApplicationDbContext _dbContext;
private readonly Mock<ICurrentUser> _currentUser;
    private readonly Mock<IFileStorage> _fileStorage;
    private readonly CreateCourseCommandHandler _handler;

    public CreateCourseCommandHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
_currentUser = new Mock<ICurrentUser>();
        _currentUser.Setup(c => c.UserId).Returns(CurrentUserId);
        _currentUser.Setup(c => c.FullName).Returns("Actual Caller");
        _currentUser.Setup(c => c.HasPermission(Permissions.Courses.ManageAll)).Returns(false);

        _fileStorage = new Mock<IFileStorage>();

        // The handler takes both the caller (for the instructor ownership check) and the
        // storage (for the optional thumbnail upload) after the two branches were merged.
        _handler = new CreateCourseCommandHandler(_dbContext, _currentUser.Object, _fileStorage.Object);
    }

    private static readonly Guid CurrentUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task Handle_WhenCategoryDoesNotExist_ReturnsNotFoundFailure()
    {
        // Arrange
        var command = new CreateCourseCommand(
            "Course Title",
            "Description",
            null,
            null,
            50,
            CourseLevel.Beginner,
            Guid.NewGuid(),
            "Instructor Name",
            Guid.NewGuid()); // Non-existent category ID

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(CategoryErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WithValidCommand_CreatesCourseAndReturnsSuccess()
    {
        // Arrange
        var category = Category.Create("Test Category", "test-category", null, null, 1).Data!;
        _dbContext.Add(category);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var instructorId = Guid.NewGuid();
        var command = new CreateCourseCommand(
            "New Course Title",
            "Description of the new course",
            null,
            null,
            100,
            CourseLevel.Intermediate,
            instructorId,
            "John Doe",
            category.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeEmpty();

        var createdCourse = await _dbContext.FirstOrDefaultAsync(
            _dbContext.Courses, c => c.Id == result.Data);

        createdCourse.Should().NotBeNull();
        createdCourse!.Title.Should().Be("New Course Title");
        createdCourse.Description.Should().Be("Description of the new course");
        createdCourse.Credits.Should().Be(100);
        createdCourse.Level.Should().Be(CourseLevel.Intermediate);
        createdCourse.CategoryId.Should().Be(category.Id);
        createdCourse.Status.Should().Be(CourseStatus.Draft);

        // The caller has no Courses.ManageAll, so the client-supplied instructor must be ignored.
        createdCourse.InstructorId.Should().Be(CurrentUserId);
        createdCourse.InstructorName.Should().Be("Actual Caller");
    }

    [Fact]
    public async Task Handle_WithoutManageAll_IgnoresClientSuppliedInstructor()
    {
        // Arrange
        var category = Category.Create("Test Category", "test-category", null, null, 1).Data!;
        _dbContext.Add(category);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var victimId = Guid.NewGuid();
        var command = new CreateCourseCommand(
            "Impostor Course",
            "Description",
            null, // ThumbnailStream
            null, // ThumbnailFileName
            100,
            CourseLevel.Beginner,
            victimId,
            "Victim Instructor",
            category.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var createdCourse = await _dbContext.FirstOrDefaultAsync(
            _dbContext.Courses, c => c.Id == result.Data);

        createdCourse!.InstructorId.Should().NotBe(victimId);
        createdCourse.InstructorId.Should().Be(CurrentUserId);
    }

    [Fact]
    public async Task Handle_WithManageAll_AllowsCreatingCourseForAnotherInstructor()
    {
        // Arrange
        _currentUser.Setup(c => c.HasPermission(Permissions.Courses.ManageAll)).Returns(true);

        var category = Category.Create("Test Category", "test-category", null, null, 1).Data!;
        _dbContext.Add(category);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var targetInstructorId = Guid.NewGuid();
        var command = new CreateCourseCommand(
            "Staff Created Course",
            "Description",
            null, // ThumbnailStream
            null, // ThumbnailFileName
            100,
            CourseLevel.Beginner,
            targetInstructorId,
            "Assigned Instructor",
            category.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var createdCourse = await _dbContext.FirstOrDefaultAsync(
            _dbContext.Courses, c => c.Id == result.Data);

        createdCourse!.InstructorId.Should().Be(targetInstructorId);
        createdCourse.InstructorName.Should().Be("Assigned Instructor");
    }

    [Fact]
    public async Task Handle_WithThumbnail_UploadsThumbnailAndSetsUrl()
    {
        // Arrange
        var category = Category.Create("Test Category", "test-category", null, null, 1).Data!;
        _dbContext.Add(category);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var instructorId = Guid.NewGuid();
        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        const string expectedUrl = "https://storage.example.com/Courses/thumbnail.png";

        _fileStorage
            .Setup(f => f.UploadAsync(stream, "thumbnail.png", "Courses"))
            .ReturnsAsync(Result<string>.Success(expectedUrl));

        var command = new CreateCourseCommand(
            "New Course With Thumbnail",
            "Description of course with thumbnail",
            stream,
            "thumbnail.png",
            150,
            CourseLevel.Advanced,
            instructorId,
            "Jane Doe",
            category.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var createdCourse = await _dbContext.FirstOrDefaultAsync(
            _dbContext.Courses, c => c.Id == result.Data);

        createdCourse.Should().NotBeNull();
        createdCourse!.ThumbnailUrl.Should().Be(expectedUrl);
    }

    [Fact]
    public async Task Handle_WithEmptyTitle_ReturnsFailure()
    {
        // Arrange
        var category = Category.Create("Test Category", "test-category", null, null, 1).Data!;
        _dbContext.Add(category);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new CreateCourseCommand(
            "", // Invalid title
            "Description",
            null,
            null,
            100,
            CourseLevel.Beginner,
            Guid.NewGuid(),
            "Instructor",
            category.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    public void Dispose()
    {
        // No-op
    }
}
