using FluentAssertions;
using Skill_Loop.Application.Common.Errors.Course;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Features.Courses.Commands.DeleteCourse;
using Skill_Loop.Domain.Enums;
using Skill_Loop.UnitTests.Common;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Skill_Loop.Domain.Entities.Courses;

namespace Skill_Loop.UnitTests.Features.Courses.Commands.DeleteCourse;

public class DeleteCourseCommandHandlerTests : IDisposable
{
    private readonly IApplicationDbContext _dbContext;
    private readonly DeleteCourseCommandHandler _handler;

    public DeleteCourseCommandHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _handler = new DeleteCourseCommandHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_WhenCourseDoesNotExist_ReturnsNotFoundFailure()
    {
        // Arrange
        var command = new DeleteCourseCommand(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(CourseErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WithValidCommand_DeletesCourseAndReturnsSuccess()
    {
        // Arrange
        var category = Category.Create("Cat", "cat", null, null, 1).Data!;
        _dbContext.Add(category);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var course = Course.Create("Title", "Desc", "img", 10, CourseLevel.Beginner, Guid.NewGuid(), "Instr", category.Id).Data!;
        _dbContext.Add(course);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new DeleteCourseCommand(course.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var deletedCourse = await _dbContext.FirstOrDefaultAsync(_dbContext.Courses, c => c.Id == course.Id);
        
        // When working with SoftDeleteInterceptor in a real DB, IsDeleted would be true.
        // But InMemory EF Core will actually remove the entity, unless intercepted. 
        // We just ensure it's either null or marked deleted.
        if (deletedCourse != null)
        {
            deletedCourse.IsDeleted.Should().BeTrue();
        }
    }

    public void Dispose()
    {
        // No-op
    }
}
