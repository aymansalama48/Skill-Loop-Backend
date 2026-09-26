using FluentAssertions;
using Moq;
using Skill_Loop.Application.Common.Abstractions.External.FileStorage;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Features.Categories.Commands.CreateCategory;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Courses;
using Skill_Loop.UnitTests.Common;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Skill_Loop.UnitTests.Features.Categories.Commands.CreateCategory;

public class CreateCategoryCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly Mock<IFileStorage> _fileStorage;
    private readonly CreateCategoryCommandHandler _handler;

    public CreateCategoryCommandHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _fileStorage = new Mock<IFileStorage>();
        _handler = new CreateCategoryCommandHandler(_dbContext, _fileStorage.Object);
    }

    [Fact]
    public async Task Handle_WithDuplicateSlug_ReturnsConflictFailure()
    {
        var existing = Category.Create("Programming", "programming").Data!;
        _dbContext.Add(existing);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new CreateCategoryCommand("Programming", "programming", null, null, null, 0);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "Category.DuplicateSlug" && e.Type == ErrorType.Conflict);
    }

    [Fact]
    public async Task Handle_WithoutIconStream_CreatesCategoryAndReturnsId()
    {
        var command = new CreateCategoryCommand(
            "Programming", "programming", null, null, "Learn to code", 1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeEmpty();

        var created = await _dbContext.FirstOrDefaultAsync(
            _dbContext.Categories.Where(c => c.Id == result.Data));

        created.Should().NotBeNull();
        created!.Name.Should().Be("Programming");
        created.Slug.Should().Be("programming");
        created.Description.Should().Be("Learn to code");
        created.DisplayOrder.Should().Be(1);
        created.IconUrl.Should().BeNull();
        _fileStorage.Verify(f => f.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithIconStream_UploadsIconAndStoresUrl()
    {
        var command = new CreateCategoryCommand(
            "Programming", "programming", new MemoryStream(), "icon.png", "Desc", 0);

        _fileStorage.Setup(f => f.UploadAsync(It.IsAny<Stream>(), "icon.png", "categories"))
            .ReturnsAsync(Result<string>.Success("categories/icon.png"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var created = await _dbContext.FirstOrDefaultAsync(
            _dbContext.Categories.Where(c => c.Id == result.Data));

        created!.IconUrl.Should().Be("categories/icon.png");
        _fileStorage.Verify(f => f.UploadAsync(It.IsAny<Stream>(), "icon.png", "categories"), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenIconUploadFails_ReturnsFailure()
    {
        var uploadError = new Error("FileStorage.UploadFailed", "Upload failed", ErrorType.Failure);
        _fileStorage.Setup(f => f.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Result<string>.Failure(uploadError));

        var command = new CreateCategoryCommand(
            "Programming", "programming", new MemoryStream(), "icon.png", null, 0);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "FileStorage.UploadFailed");
    }
}
