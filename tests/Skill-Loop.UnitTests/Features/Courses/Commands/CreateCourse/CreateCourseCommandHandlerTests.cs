using FluentAssertions;
using Skill_Loop.Application.Common.Errors.Category;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Features.Courses.Commands.CreateCourse;
using Skill_Loop.Domain.Enums;
using Skill_Loop.UnitTests.Common;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Skill_Loop.Domain.Entities.Courses;

namespace Skill_Loop.UnitTests.Features.Courses.Commands.CreateCourse;

public class CreateCourseCommandHandlerTests : IDisposable
{
    private readonly IApplicationDbContext _dbContext;
    private readonly CreateCourseCommandHandler _handler;

    public CreateCourseCommandHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _handler = new CreateCourseCommandHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_WhenCategoryDoesNotExist_ReturnsNotFoundFailure()
    {
        // Arrange
        var command = new CreateCourseCommand(
            "Course Title",
            "Description",
            "https://img.com/img.png",
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
            "https://img.com/img.png",
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
        createdCourse.InstructorId.Should().Be(instructorId);
        createdCourse.InstructorName.Should().Be("John Doe");
        createdCourse.CategoryId.Should().Be(category.Id);
        createdCourse.Status.Should().Be(CourseStatus.Draft);
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
            "https://img.com/img.png",
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
