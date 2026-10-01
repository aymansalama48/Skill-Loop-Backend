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
    private readonly UpdateCategoryCommandHandler _handler;
    public UpdateCategoryCommandHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _handler = new UpdateCategoryCommandHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_WhenCategoryDoesNotExist_ReturnsNotFoundFailure()
    {
        var command = new UpdateCategoryCommand(
            Guid.NewGuid(), "Name", null, 0);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "Category.NotFound" && e.Type == ErrorType.NotFound);
    }


    [Fact]
    public async Task Handle_WithoutIconStream_UpdatesCategoryAndReturnsSuccess()
    {
        var category = Category.Create("Programming", "old-icon.png", "Desc", 1).Data!;
        _dbContext.Add(category);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new UpdateCategoryCommand(
            category.Id, "Advanced Programming", "New desc", 5);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var updated = await _dbContext.FirstOrDefaultAsync(
            _dbContext.Categories.Where(c => c.Id == category.Id));
        updated!.Name.Should().Be("Advanced Programming");
        updated.Description.Should().Be("New desc");
        updated.DisplayOrder.Should().Be(5);
        updated.IconUrl.Should().Be("old-icon.png");
    }

}
