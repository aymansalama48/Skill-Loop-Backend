using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Instructors;
using Skill_Loop.Application.Features.Instructors.Commands.UpdateInstructorReview;
using Skill_Loop.Domain.Entities.Instructors;
using Skill_Loop.UnitTests.Common;

namespace Skill_Loop.UnitTests.Features.Instructors.Commands.UpdateInstructorReview;

public class UpdateInstructorReviewCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly UpdateInstructorReviewCommandHandler _handler;

    public UpdateInstructorReviewCommandHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _handler = new UpdateInstructorReviewCommandHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_WhenProfileDoesNotExist_ReturnsNotFoundFailure()
    {
        var command = new UpdateInstructorReviewCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 4, "Updated");

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

        var command = new UpdateInstructorReviewCommand(
            profile.Id, Guid.NewGuid(), Guid.NewGuid(), 5, "Updated");

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

        profile.AddReview(authorId, 3, "Original");
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var reviewId = profile.Reviews.Single().Id;

        var command = new UpdateInstructorReviewCommand(profile.Id, reviewId, otherUserId, 5, "Hacked");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "InstructorReview.Unauthorized");
    }

    [Fact]
    public async Task Handle_WithValidRequest_UpdatesReviewAndRecalculatesRating()
    {
        var userId = Guid.NewGuid();
        var learnerId = Guid.NewGuid();

        var profile = InstructorProfile.Create(userId, "H", "B").Data!;
        _dbContext.Add(profile);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        profile.AddReview(learnerId, 3, "Okay");
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var reviewId = profile.Reviews.Single().Id;

        var command = new UpdateInstructorReviewCommand(profile.Id, reviewId, learnerId, 5, "Much better");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var persisted = await _dbContext.FirstOrDefaultAsync(
            _dbContext.InstructorProfiles
                .Include(p => p.Reviews)
                .Where(p => p.Id == profile.Id));

        persisted!.Reviews.Single().Comment.Should().Be("Much better");
        persisted.Rating.Should().Be(5.0);
    }
}
