using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Instructors;
using Skill_Loop.Application.Features.Instructors.Commands.AddInstructorReview;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Instructors;
using Skill_Loop.UnitTests.Common;

namespace Skill_Loop.UnitTests.Features.Instructors.Commands.AddInstructorReview;

public class AddInstructorReviewCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly AddInstructorReviewCommandHandler _handler;

    public AddInstructorReviewCommandHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _handler = new AddInstructorReviewCommandHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_WhenProfileDoesNotExist_ReturnsNotFoundFailure()
    {
        var command = new AddInstructorReviewCommand(
            Guid.NewGuid(), Guid.NewGuid(), 5, "Great!");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == InstructorProfileErrors.NotFound.Code);
    }

    [Fact]
    public async Task Handle_WhenLearnerReviewsSelf_ReturnsConflictFailure()
    {
        var userId = Guid.NewGuid();
        var profile = InstructorProfile.Create(userId, "Headline", "Bio").Data!;
        _dbContext.Add(profile);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new AddInstructorReviewCommand(profile.Id, userId, 5, "Self review");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "InstructorReview.CannotReviewSelf");
    }

    [Fact]
    public async Task Handle_WhenAlreadyReviewed_ReturnsConflictFailure()
    {
        var userId = Guid.NewGuid();
        var learnerId = Guid.NewGuid();
        var profile = InstructorProfile.Create(userId, "Headline", "Bio").Data!;
        _dbContext.Add(profile);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        profile.AddReview(learnerId, 4, "First");
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new AddInstructorReviewCommand(profile.Id, learnerId, 5, "Second");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "InstructorProfile.AlreadyReviewed");
    }

    [Fact]
    public async Task Handle_WithValidRequest_AddsReviewAndRecalculatesRating()
    {
        var userId = Guid.NewGuid();
        var learnerId = Guid.NewGuid();
        var profile = InstructorProfile.Create(userId, "Headline", "Bio").Data!;
        _dbContext.Add(profile);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new AddInstructorReviewCommand(profile.Id, learnerId, 5, "Excellent instructor!");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeEmpty();

        var persisted = await _dbContext
            .FirstOrDefaultAsync(_dbContext.InstructorProfiles
                .Include(p => p.Reviews)
                .Where(p => p.Id == profile.Id));

        persisted!.Reviews.Should().ContainSingle(r => r.Id == result.Data);
        persisted.Rating.Should().Be(5.0);
    }

    [Fact]
    public async Task Handle_WithNullComment_AddsReviewSuccessfully()
    {
        var userId = Guid.NewGuid();
        var learnerId = Guid.NewGuid();
        var profile = InstructorProfile.Create(userId, "Headline", "Bio").Data!;
        _dbContext.Add(profile);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new AddInstructorReviewCommand(profile.Id, learnerId, 3, null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}
