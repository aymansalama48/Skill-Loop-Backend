using FluentAssertions;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Instructors;
using Skill_Loop.Application.Features.Instructors.Commands.ChangeInstructorApprovalStatus;
using Skill_Loop.Domain.Entities.Instructors;
using Skill_Loop.UnitTests.Common;

namespace Skill_Loop.UnitTests.Features.Instructors.Commands.ChangeInstructorApprovalStatus;

public class ChangeInstructorApprovalStatusCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ChangeInstructorApprovalStatusCommandHandler _handler;

    public ChangeInstructorApprovalStatusCommandHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _handler = new ChangeInstructorApprovalStatusCommandHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_WhenProfileDoesNotExist_ReturnsNotFoundFailure()
    {
        var userId = Guid.NewGuid();
        var command = new ChangeInstructorApprovalStatusCommand(userId, IsApproved: true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == InstructorProfileErrors.NotFound.Code);
    }

    [Fact]
    public async Task Handle_WhenApproving_ApprovesProfileAndReturnsSuccess()
    {
        var userId = Guid.NewGuid();
        var profile = InstructorProfile.Create(userId, "Headline", "Bio").Data!;
        _dbContext.Add(profile);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new ChangeInstructorApprovalStatusCommand(userId, IsApproved: true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var updated = await _dbContext.FirstOrDefaultAsync(
            _dbContext.InstructorProfiles.Where(p => p.UserId == userId));
        updated!.IsApproved.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenSuspending_SuspendsProfileAndReturnsSuccess()
    {
        var userId = Guid.NewGuid();
        var profile = InstructorProfile.Create(userId, "Headline", "Bio").Data!;
        profile.Approve();
        _dbContext.Add(profile);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new ChangeInstructorApprovalStatusCommand(userId, IsApproved: false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var updated = await _dbContext.FirstOrDefaultAsync(
            _dbContext.InstructorProfiles.Where(p => p.UserId == userId));
        updated!.IsApproved.Should().BeFalse();
    }
}
