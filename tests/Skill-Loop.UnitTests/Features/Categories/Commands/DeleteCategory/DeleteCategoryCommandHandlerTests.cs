using FluentAssertions;
using Moq;
using Skill_Loop.Application.Common.Abstractions.External.FileStorage;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Features.Categories.Commands.DeleteCategory;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Courses;

using Skill_Loop.Domain.Enums;
using Skill_Loop.UnitTests.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Skill_Loop.UnitTests.Features.Categories.Commands.DeleteCategory;

public class DeleteCategoryCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly Mock<IFileStorage> _fileStorage;
    private readonly DeleteCategoryCommandHandler _handler;

    public DeleteCategoryCommandHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _fileStorage = new Mock<IFileStorage>();
        _handler = new DeleteCategoryCommandHandler(_dbContext, _fileStorage.Object);
    }

    [Fact]
    public async Task Handle_WhenCategoryDoesNotExist_ReturnsNotFoundFailure()
    {
        var command = new DeleteCategoryCommand(Guid.NewGuid());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "Category.NotFound" && e.Type == ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_WhenCategoryHasCourses_ReturnsConflictFailure()
    {
        var category = Category.Create("Programming").Data!;
        _dbContext.Add(category);

        var course = Course.Create(
            "C#", "Desc", "https://img.png", 50, CourseLevel.Beginner,
            Guid.NewGuid(), "Instructor", category.Id).Data!;
        _dbContext.Add(course);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new DeleteCategoryCommand(category.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "Category.HasCourses" && e.Type == ErrorType.Conflict);

        var existing = await _dbContext.FirstOrDefaultAsync(
            _dbContext.Categories.Where(c => c.Id == category.Id));
        existing.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithNoCourses_DeletesCategoryAndReturnsSuccess()
    {
        var category = Category.Create("Programming").Data!;
        _dbContext.Add(category);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new DeleteCategoryCommand(category.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var deleted = await _dbContext.FirstOrDefaultAsync(
            _dbContext.Categories.Where(c => c.Id == category.Id));
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithIconUrl_DeletesIconFromStorage()
    {
        var category = Category.Create("Programming", "categories/icon.png", null, 0).Data!;
        _dbContext.Add(category);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new DeleteCategoryCommand(category.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _fileStorage.Verify(f => f.DeleteAsync("categories/icon.png"), Times.Once);
    }

    [Fact]
    public async Task Handle_WithoutIconUrl_DoesNotCallDeleteOnStorage()
    {
        var category = Category.Create("Programming", null, null, 0).Data!;
        _dbContext.Add(category);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new DeleteCategoryCommand(category.Id);

        await _handler.Handle(command, CancellationToken.None);

        _fileStorage.Verify(f => f.DeleteAsync(It.IsAny<string>()), Times.Never);
    }
}
