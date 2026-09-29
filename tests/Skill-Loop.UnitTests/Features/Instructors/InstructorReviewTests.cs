using FluentAssertions;
using Skill_Loop.Domain.Entities.Instructors;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.UnitTests.Features.Instructors;

public class InstructorReviewTests
{
    [Fact]
    public void Create_WithValidRating_ShouldCreateReviewWithAllProperties()
    {
        var profileId = Guid.NewGuid();
        var learnerId = Guid.NewGuid();

        var result = InstructorReview.Create(profileId, learnerId, 4, "Great instructor!");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.InstructorProfileId.Should().Be(profileId);
        result.Data.LearnerUserId.Should().Be(learnerId);
        result.Data.Rating.Should().Be(4);
        result.Data.Comment.Should().Be("Great instructor!");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void Create_WithRatingOutside1To5_ShouldReturnValidationError(int rating)
    {
        var result = InstructorReview.Create(Guid.NewGuid(), Guid.NewGuid(), rating, "comment");

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "InstructorReview.InvalidRating");
    }

    [Fact]
    public void Create_BoundaryRating1_ShouldSucceed()
    {
        var result = InstructorReview.Create(Guid.NewGuid(), Guid.NewGuid(), 1, "min");

        result.IsSuccess.Should().BeTrue();
        result.Data!.Rating.Should().Be(1);
    }

    [Fact]
    public void Create_BoundaryRating5_ShouldSucceed()
    {
        var result = InstructorReview.Create(Guid.NewGuid(), Guid.NewGuid(), 5, "max");

        result.IsSuccess.Should().BeTrue();
        result.Data!.Rating.Should().Be(5);
    }

    [Fact]
    public void Create_WithNullComment_ShouldSetEmptyString()
    {
        var result = InstructorReview.Create(Guid.NewGuid(), Guid.NewGuid(), 3, null);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Comment.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithWhitespaceComment_ShouldTrimToEmptyString()
    {
        var result = InstructorReview.Create(Guid.NewGuid(), Guid.NewGuid(), 3, "   ");

        result.IsSuccess.Should().BeTrue();
        result.Data!.Comment.Should().BeEmpty();
    }

    [Fact]
    public void Update_ShouldReplaceRatingAndComment()
    {
        var review = InstructorReview.Create(
            Guid.NewGuid(), Guid.NewGuid(), 3, "original").Data!;

        review.Update(5, "updated comment");

        review.Rating.Should().Be(5);
        review.Comment.Should().Be("updated comment");
    }

    [Fact]
    public void Update_WithNullComment_ShouldSetEmptyString()
    {
        var review = InstructorReview.Create(
            Guid.NewGuid(), Guid.NewGuid(), 3, "original").Data!;

        review.Update(4, null);

        review.Comment.Should().BeEmpty();
        review.Rating.Should().Be(4);
    }
}
