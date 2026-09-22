using FluentAssertions;
using Skill_Loop.Domain.Entities.Courses;
using Skill_Loop.Domain.Entities.Courses.Events;
using Skill_Loop.Domain.Entities.Courses.ValueObjects;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.UnitTests.Features.Courses;

public class CourseAggregateTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateCourseAndRaiseDomainEvent()
    {
        // Arrange
        var instructorId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var price = CoursePrice.Create(50).Data!;

        // Act
        var result = Course.Create(
            "Full-Stack Web Development",
            "Master React and ASP.NET Core from scratch.",
            "https://cdn.skillloop.com/thumbnails/fullstack.png",
            price,
            CourseLevel.Beginner,
            instructorId,
            "John Doe",
            categoryId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Title.Should().Be("Full-Stack Web Development");
        result.Data.Price.Credits.Should().Be(50);
        result.Data.Status.Should().Be(CourseStatus.Draft);
        result.Data.DomainEvents.Should().ContainSingle(e => e is CourseCreatedDomainEvent);
    }

    [Fact]
    public void CoursePrice_WithNegativeCredits_ShouldReturnValidationError()
    {
        // Act
        var result = CoursePrice.Create(-10);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "CoursePrice.Negative");
    }

    [Fact]
    public void AddLessonToSection_ShouldIncrementTotalLessonsCountAndDuration()
    {
        // Arrange
        var course = Course.Create(
            "UI/UX Design Masterclass",
            "Learn Figma and Prototyping.",
            "https://cdn.skillloop.com/thumbnails/uiux.png",
            CoursePrice.Free(),
            CourseLevel.Intermediate,
            Guid.NewGuid(),
            "Hala Designer",
            Guid.NewGuid()).Data!;

        course.AddSection("Introduction", 0);
        var section = course.Sections.First();

        var video = VideoResource.Create(
            "https://cdn.skillloop.com/videos/lesson1.mp4",
            TimeSpan.FromMinutes(15)).Data!;

        // Act
        var addResult = course.AddLessonToSection(section.Id, "Welcome & Overview", video, 0, isPreviewable: true);

        // Assert
        addResult.IsSuccess.Should().BeTrue();
        course.TotalLessonsCount.Should().Be(1);
        course.TotalDuration.Should().Be(TimeSpan.FromMinutes(15));
    }

    [Fact]
    public void AddReview_ShouldRecalculateAverageRating()
    {
        // Arrange
        var course = Course.Create(
            "AI & Machine Learning",
            "Intro to Python, PyTorch and LLMs.",
            "https://cdn.skillloop.com/thumbnails/ai.png",
            CoursePrice.Create(100).Data!,
            CourseLevel.Advanced,
            Guid.NewGuid(),
            "Dr. AI",
            Guid.NewGuid()).Data!;

        // Act
        course.AddReview(Guid.NewGuid(), 5, "Amazing course!");
        course.AddReview(Guid.NewGuid(), 3, "Good overview.");

        // Assert
        course.Rating.TotalReviews.Should().Be(2);
        course.Rating.AverageRating.Should().Be(4.0);
    }
}
