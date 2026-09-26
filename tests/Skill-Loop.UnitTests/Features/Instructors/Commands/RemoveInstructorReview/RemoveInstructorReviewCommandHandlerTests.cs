using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Instructors;
using Skill_Loop.Application.Features.Instructors.Commands.RemoveInstructorReview;
using Skill_Loop.Domain.Entities.Instructors;
using Skill_Loop.UnitTests.Common;

namespace Skill_Loop.UnitTests.Features.Instructors.Commands.RemoveInstructorReview;

public class RemoveInstructorReviewCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly RemoveInstructorReviewCommandHandler _handler;

    public RemoveInstructorReviewCommandHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _handler = new RemoveInstructorReviewCommandHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_WhenProfileDoesNotExist_ReturnsNotFoundFailure()
    {
        var command = new RemoveInstructorReviewCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == InstructorProfileErrors.NotFound.Code);
    }

    [Fact]
    public async Task Handle_WhenReviewDoesNotExist_ReturnsNotFoundFailure()
    {
        var userId = Guid.NewGuid();
        var profile = InstructorProfile.Create(userId, "H", "B").Data!;
        _dbContext.Add(profile);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new RemoveInstructorReviewCommand(profile.Id, Guid.NewGuid(), Guid.NewGuid());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "InstructorReview.NotFound");
    }

    [Fact]
    public async Task Handle_WhenNotReviewer_ReturnsUnauthorizedFailure()
    {
        var userId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var profile = InstructorProfile.Create(userId, "H", "B").Data!;
        _dbContext.Add(profile);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        profile.AddReview(authorId, 4, "Original");
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var reviewId = profile.Reviews.Single().Id;

        var command = new RemoveInstructorReviewCommand(profile.Id, reviewId, otherUserId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "InstructorReview.Unauthorized");
    }

    [Fact]
    public async Task Handle_WithValidRequest_RemovesReviewAndRecalculatesRating()
    {
        var userId = Guid.NewGuid();
        var learner1 = Guid.NewGuid();
        var learner2 = Guid.NewGuid();

        var profile = InstructorProfile.Create(userId, "H", "B").Data!;
        _dbContext.Add(profile);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        profile.AddReview(learner1, 5, "Great");
        profile.AddReview(learner2, 3, "Okay");
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var reviewId = profile.Reviews.First(r => r.LearnerUserId == learner1).Id;

        var command = new RemoveInstructorReviewCommand(profile.Id, reviewId, learner1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var persisted = await _dbContext.FirstOrDefaultAsync(
            _dbContext.InstructorProfiles
                .Include(p => p.Reviews)
                .Where(p => p.Id == profile.Id));

        persisted!.Reviews.Should().HaveCount(1);
        persisted.Rating.Should().Be(3.0);
    }
}
