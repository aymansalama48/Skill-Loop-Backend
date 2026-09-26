using FluentAssertions;
using Skill_Loop.Domain.Entities.Instructors;

namespace Skill_Loop.UnitTests.Features.Instructors;

public class InstructorProfileTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateProfileWithDefaultState()
    {
        var userId = Guid.NewGuid();

        var result = InstructorProfile.Create(userId, "Senior .NET Developer", "Experienced instructor.");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.UserId.Should().Be(userId);
        result.Data.Headline.Should().Be("Senior .NET Developer");
        result.Data.Bio.Should().Be("Experienced instructor.");
        result.Data.IsApproved.Should().BeFalse();
        result.Data.Rating.Should().Be(0.0);
        result.Data.SessionsCompleted.Should().Be(0);
        result.Data.CreditsEarned.Should().Be(0);
        result.Data.Reviews.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldReturnValidationError()
    {
        var result = InstructorProfile.Create(Guid.Empty, "Headline", "Bio");

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "InstructorProfile.InvalidUser");
    }

    [Fact]
    public void UpdateDetails_ShouldUpdateHeadlineAndBio()
    {
        var profile = InstructorProfile.Create(
            Guid.NewGuid(), "Old Headline", "Old Bio").Data!;

        var result = profile.UpdateDetails("New Headline", "New Bio");

        result.IsSuccess.Should().BeTrue();
        profile.Headline.Should().Be("New Headline");
        profile.Bio.Should().Be("New Bio");
    }

    [Fact]
    public void Approve_ShouldSetIsApprovedToTrue()
    {
        var profile = InstructorProfile.Create(Guid.NewGuid(), "H", "B").Data!;

        profile.Approve();

        profile.IsApproved.Should().BeTrue();
    }

    [Fact]
    public void Suspend_ShouldSetIsApprovedToFalse()
    {
        var profile = InstructorProfile.Create(Guid.NewGuid(), "H", "B").Data!;
        profile.Approve();

        profile.Suspend();

        profile.IsApproved.Should().BeFalse();
    }

    [Fact]
    public void IncrementSessionsCompleted_ShouldIncrementCount()
    {
        var profile = InstructorProfile.Create(Guid.NewGuid(), "H", "B").Data!;

        profile.IncrementSessionsCompleted();
        profile.IncrementSessionsCompleted();

        profile.SessionsCompleted.Should().Be(2);
    }

    [Fact]
    public void AddCreditsEarned_ShouldAccumulateCredits()
    {
        var profile = InstructorProfile.Create(Guid.NewGuid(), "H", "B").Data!;

        profile.AddCreditsEarned(40);
        profile.AddCreditsEarned(10);

        profile.CreditsEarned.Should().Be(50);
    }

    [Fact]
    public void AddReview_WithValidReview_ShouldAddReviewAndRecalculateRating()
    {
        var profile = InstructorProfile.Create(Guid.NewGuid(), "H", "B").Data!;
        var learnerId = Guid.NewGuid();

        var result = profile.AddReview(learnerId, 4, "Good!");

        result.IsSuccess.Should().BeTrue();
        profile.Reviews.Should().ContainSingle();
        profile.Rating.Should().Be(4.0);
    }

    [Fact]
    public void AddReview_MultipleReviews_ShouldRecalculateAverageRating()
    {
        var profile = InstructorProfile.Create(Guid.NewGuid(), "H", "B").Data!;

        profile.AddReview(Guid.NewGuid(), 5, "Excellent");
        profile.AddReview(Guid.NewGuid(), 3, "Okay");

        profile.Rating.Should().Be(4.0);
    }

    [Fact]
    public void AddReview_WhenAlreadyReviewed_ShouldReturnConflictFailure()
    {
        var profile = InstructorProfile.Create(Guid.NewGuid(), "H", "B").Data!;
        var learnerId = Guid.NewGuid();

        profile.AddReview(learnerId, 5, "First");
        var result = profile.AddReview(learnerId, 3, "Second");

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "InstructorProfile.AlreadyReviewed");
        profile.Reviews.Should().ContainSingle();
    }

    [Fact]
    public void UpdateReview_WithValidData_ShouldUpdateAndRecalculateRating()
    {
        var profile = InstructorProfile.Create(Guid.NewGuid(), "H", "B").Data!;
        var learnerId = Guid.NewGuid();

        profile.AddReview(learnerId, 3, "Okay");
        var reviewId = profile.Reviews.Single().Id;

        var result = profile.UpdateReview(reviewId, learnerId, 5, "Much better now");

        result.IsSuccess.Should().BeTrue();
        profile.Rating.Should().Be(5.0);
        profile.Reviews.Single().Comment.Should().Be("Much better now");
    }

    [Fact]
    public void UpdateReview_WhenReviewNotFound_ShouldReturnNotFoundFailure()
    {
        var profile = InstructorProfile.Create(Guid.NewGuid(), "H", "B").Data!;
        profile.AddReview(Guid.NewGuid(), 4, "Good");

        var result = profile.UpdateReview(Guid.NewGuid(), Guid.NewGuid(), 5, "Updated");

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "InstructorReview.NotFound");
    }

    [Fact]
    public void UpdateReview_WhenNotReviewer_ShouldReturnUnauthorizedFailure()
    {
        var profile = InstructorProfile.Create(Guid.NewGuid(), "H", "B").Data!;
        var authorId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        profile.AddReview(authorId, 4, "Good");
        var reviewId = profile.Reviews.Single().Id;

        var result = profile.UpdateReview(reviewId, otherUserId, 5, "Hacked");

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "InstructorReview.Unauthorized");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void UpdateReview_WithInvalidRating_ShouldReturnValidationError(int rating)
    {
        var profile = InstructorProfile.Create(Guid.NewGuid(), "H", "B").Data!;
        var learnerId = Guid.NewGuid();

        profile.AddReview(learnerId, 4, "Good");
        var reviewId = profile.Reviews.Single().Id;

        var result = profile.UpdateReview(reviewId, learnerId, rating, "Updated");

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "InstructorReview.InvalidRating");
    }

    [Fact]
    public void RemoveReview_WithValidRequest_ShouldRemoveAndRecalculateRating()
    {
        var profile = InstructorProfile.Create(Guid.NewGuid(), "H", "B").Data!;
        var learnerId = Guid.NewGuid();

        profile.AddReview(learnerId, 5, "Excellent");
        profile.AddReview(Guid.NewGuid(), 3, "Okay");

        var reviewId = profile.Reviews.First(r => r.LearnerUserId == learnerId).Id;

        var result = profile.RemoveReview(reviewId, learnerId);

        result.IsSuccess.Should().BeTrue();
        profile.Reviews.Should().HaveCount(1);
        profile.Rating.Should().Be(3.0);
    }

    [Fact]
    public void RemoveReview_WhenReviewNotFound_ShouldReturnNotFoundFailure()
    {
        var profile = InstructorProfile.Create(Guid.NewGuid(), "H", "B").Data!;
        profile.AddReview(Guid.NewGuid(), 4, "Good");

        var result = profile.RemoveReview(Guid.NewGuid(), Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "InstructorReview.NotFound");
    }

    [Fact]
    public void RemoveReview_WhenNotReviewer_ShouldReturnUnauthorizedFailure()
    {
        var profile = InstructorProfile.Create(Guid.NewGuid(), "H", "B").Data!;
        var authorId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        profile.AddReview(authorId, 4, "Good");
        var reviewId = profile.Reviews.Single().Id;

        var result = profile.RemoveReview(reviewId, otherUserId);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "InstructorReview.Unauthorized");
    }

    [Fact]
    public void RemoveAllReviews_ShouldResetRatingToZero()
    {
        var profile = InstructorProfile.Create(Guid.NewGuid(), "H", "B").Data!;

        profile.AddReview(Guid.NewGuid(), 5, "Great");
        profile.AddReview(Guid.NewGuid(), 4, "Good");

        foreach (var review in profile.Reviews.ToList())
        {
            profile.RemoveReview(review.Id, review.LearnerUserId);
        }

        profile.Rating.Should().Be(0.0);
        profile.Reviews.Should().BeEmpty();
    }
}
