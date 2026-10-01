using FluentAssertions;
using Skill_Loop.Domain.Entities.Courses;
using Skill_Loop.Domain.Entities.Courses.Events;

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
        // Act
        var result = Course.Create(
            "Full-Stack Web Development",
            "Master React and ASP.NET Core from scratch.",
            "https://cdn.skillloop.com/thumbnails/fullstack.png",
            50,
            CourseLevel.Beginner,
            instructorId,
            "John Doe",
            categoryId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Title.Should().Be("Full-Stack Web Development");
        result.Data.Credits.Should().Be(50);
        result.Data.Status.Should().Be(CourseStatus.Draft);
        result.Data.DomainEvents.Should().ContainSingle(e => e is CourseCreatedDomainEvent);
    }



    [Fact]
    public void AddLessonToSection_ShouldIncrementTotalLessonsCount()
    {
        // Arrange
        var course = Course.Create(
            "UI/UX Design Masterclass",
            "Learn Figma and Prototyping.",
            "https://cdn.skillloop.com/thumbnails/uiux.png",
            0,
            CourseLevel.Intermediate,
            Guid.NewGuid(),
            "Hala Designer",
            Guid.NewGuid()).Data!;

        course.AddSection("Introduction", 0);
        var section = course.Sections.First();

        var addResult = course.AddLessonToSection(section.Id, "Welcome & Overview", 0, isPreviewable: true);

        // Assert
        addResult.IsSuccess.Should().BeTrue();
        course.TotalLessonsCount.Should().Be(1);
    }

    [Fact]
    public void UpdateLessonVideo_ShouldUpdateTotalDuration()
    {
        // Arrange
        var course = Course.Create(
            "UI/UX Design Masterclass",
            "Learn Figma and Prototyping.",
            "https://cdn.skillloop.com/thumbnails/uiux.png",
            0,
            CourseLevel.Intermediate,
            Guid.NewGuid(),
            "Hala Designer",
            Guid.NewGuid()).Data!;

        course.AddSection("Introduction", 0);
        var section = course.Sections.First();
        course.AddLessonToSection(section.Id, "Welcome & Overview", 0, isPreviewable: true);
        var lesson = section.Lessons.First();

        // Act
        var updateResult = course.UpdateLessonVideo(section.Id, lesson.Id, "https://video.url", TimeSpan.FromMinutes(15), "1080p", "providerId");

        // Assert
        updateResult.IsSuccess.Should().BeTrue();
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
            100,
            CourseLevel.Advanced,
            Guid.NewGuid(),
            "Dr. AI",
            Guid.NewGuid()).Data!;

        // Act
        course.AddReview(Guid.NewGuid(), 5, "Amazing course!");
        course.AddReview(Guid.NewGuid(), 3, "Good overview.");

        // Assert
        course.TotalReviews.Should().Be(2);
        course.AverageRating.Should().Be(4.0);
    }
}
