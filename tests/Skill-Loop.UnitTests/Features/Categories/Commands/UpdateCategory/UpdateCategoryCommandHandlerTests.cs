using FluentAssertions;
using Moq;
using Skill_Loop.Application.Common.Abstractions.External.FileStorage;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Features.Categories.Commands.UpdateCategory;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Courses;
using Skill_Loop.UnitTests.Common;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Skill_Loop.UnitTests.Features.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly Mock<IFileStorage> _fileStorage;
    private readonly UpdateCategoryCommandHandler _handler;

    public UpdateCategoryCommandHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _fileStorage = new Mock<IFileStorage>();
        _handler = new UpdateCategoryCommandHandler(_dbContext, _fileStorage.Object);
    }

    [Fact]
    public async Task Handle_WhenCategoryDoesNotExist_ReturnsNotFoundFailure()
    {
        var command = new UpdateCategoryCommand(
            Guid.NewGuid(), "Name", "slug", null, null, null, 0);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "Category.NotFound" && e.Type == ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_WithDuplicateSlug_ReturnsConflictFailure()
    {
        var cat1 = Category.Create("Programming", "programming").Data!;
        var cat2 = Category.Create("Math", "math").Data!;
        _dbContext.Add(cat1);
        _dbContext.Add(cat2);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new UpdateCategoryCommand(
            cat2.Id, "Math Updated", "programming", null, null, null, 0);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "Category.DuplicateSlug" && e.Type == ErrorType.Conflict);
    }

    [Fact]
    public async Task Handle_WithoutIconStream_UpdatesCategoryAndReturnsSuccess()
    {
        var category = Category.Create("Programming", "programming", "old-icon.png", "Desc", 1).Data!;
        _dbContext.Add(category);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new UpdateCategoryCommand(
            category.Id, "Advanced Programming", "advanced-programming", null, null, "New desc", 5);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var updated = await _dbContext.FirstOrDefaultAsync(
            _dbContext.Categories.Where(c => c.Id == category.Id));
        updated!.Name.Should().Be("Advanced Programming");
        updated.Slug.Should().Be("advanced-programming");
        updated.Description.Should().Be("New desc");
        updated.DisplayOrder.Should().Be(5);
        updated.IconUrl.Should().Be("old-icon.png");
    }

    [Fact]
    public async Task Handle_WithIconStream_DeletesOldIconAndUploadsNewOne()
    {
        var category = Category.Create("Programming", "programming", "old-icon.png", null, 0).Data!;
        _dbContext.Add(category);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _fileStorage.Setup(f => f.DeleteAsync("old-icon.png"))
            .ReturnsAsync(Result.Success());
        _fileStorage.Setup(f => f.UploadAsync(It.IsAny<Stream>(), "new-icon.png", "categories"))
            .ReturnsAsync(Result<string>.Success("categories/new-icon.png"));

        var command = new UpdateCategoryCommand(
            category.Id, "Programming", "programming", new MemoryStream(), "new-icon.png", null, 0);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _fileStorage.Verify(f => f.DeleteAsync("old-icon.png"), Times.Once);
        _fileStorage.Verify(f => f.UploadAsync(It.IsAny<Stream>(), "new-icon.png", "categories"), Times.Once);

        var updated = await _dbContext.FirstOrDefaultAsync(
            _dbContext.Categories.Where(c => c.Id == category.Id));
        updated!.IconUrl.Should().Be("categories/new-icon.png");
    }

    [Fact]
    public async Task Handle_WhenIconUploadFails_ReturnsFailure()
    {
        var category = Category.Create("Programming", "programming", "old-icon.png", null, 0).Data!;
        _dbContext.Add(category);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _fileStorage.Setup(f => f.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Result<string>.Failure(new Error("Upload.Failed", "failed", ErrorType.Failure)));

        var command = new UpdateCategoryCommand(
            category.Id, "Programming", "programming", new MemoryStream(), "new-icon.png", null, 0);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "Upload.Failed");
    }
}
