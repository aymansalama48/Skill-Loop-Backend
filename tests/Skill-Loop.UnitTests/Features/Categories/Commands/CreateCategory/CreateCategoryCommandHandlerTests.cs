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
    private readonly CreateCategoryCommandHandler _handler;
    public CreateCategoryCommandHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _handler = new CreateCategoryCommandHandler(_dbContext);
    }


    [Fact]
    public async Task Handle_WithoutIconStream_CreatesCategoryAndReturnsId()
    {
        var command = new CreateCategoryCommand(
            "Programming", "Learn to code", 1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeEmpty();

        var created = await _dbContext.FirstOrDefaultAsync(
            _dbContext.Categories.Where(c => c.Id == result.Data));

        created.Should().NotBeNull();
        created!.Name.Should().Be("Programming");
        created.Description.Should().Be("Learn to code");
        created.DisplayOrder.Should().Be(1);
    }

}
