using FluentAssertions;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Instructors;
using Skill_Loop.Application.Features.Instructors.Commands.UpdateMyInstructorProfile;
using Skill_Loop.Domain.Entities.Instructors;
using Skill_Loop.UnitTests.Common;

namespace Skill_Loop.UnitTests.Features.Instructors.Commands.UpdateMyInstructorProfile;

public class UpdateMyInstructorProfileCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly UpdateMyInstructorProfileCommandHandler _handler;

    public UpdateMyInstructorProfileCommandHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _handler = new UpdateMyInstructorProfileCommandHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_WhenProfileDoesNotExist_ReturnsNotFoundFailure()
    {
        var userId = Guid.NewGuid();
        var command = new UpdateMyInstructorProfileCommand(userId, "New Headline", "New Bio");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == InstructorProfileErrors.NotFound.Code);
    }

    [Fact]
    public async Task Handle_WhenProfileExists_UpdatesDetailsAndReturnsSuccess()
    {
        var userId = Guid.NewGuid();
        var profile = InstructorProfile.Create(userId, "Old Headline", "Old Bio").Data!;
        _dbContext.Add(profile);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new UpdateMyInstructorProfileCommand(userId, "Updated Headline", "Updated Bio");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var updated = await _dbContext.FirstOrDefaultAsync(
            _dbContext.InstructorProfiles.Where(p => p.UserId == userId));
        updated.Should().NotBeNull();
        updated!.Headline.Should().Be("Updated Headline");
        updated.Bio.Should().Be("Updated Bio");
    }
}
