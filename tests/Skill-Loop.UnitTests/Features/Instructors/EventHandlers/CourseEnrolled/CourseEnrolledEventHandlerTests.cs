using FluentAssertions;
using Moq;
using Skill_Loop.Application.Common.Abstractions.Events;
using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Features.Instructors.EventHandlers.CourseEnrolled;
using Skill_Loop.Domain.Entities.Courses;
using Skill_Loop.Domain.Entities.Courses.ValueObjects;
using Skill_Loop.Domain.Entities.Enrollments.Events;
using Skill_Loop.Domain.Entities.Instructors;
using Skill_Loop.Domain.Enums;
using Skill_Loop.UnitTests.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Skill_Loop.UnitTests.Features.Instructors.EventHandlers.CourseEnrolled;

public class CourseEnrolledEventHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly Mock<ICacheService> _cacheService;
    private readonly CourseEnrolledEventHandler _handler;

    public CourseEnrolledEventHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _cacheService = new Mock<ICacheService>();
        _handler = new CourseEnrolledEventHandler(_dbContext, _cacheService.Object);
    }

    [Fact]
    public async Task Handle_WhenCreditsPaidIsZero_ReturnsEarlyWithoutAddingCredits()
    {
        var notification = new DomainEventNotification<CourseEnrolledDomainEvent>(
            new CourseEnrolledDomainEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 0));

        await _handler.Handle(notification, CancellationToken.None);

        _cacheService.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCourseDoesNotExist_ReturnsEarly()
    {
        var instructorId = Guid.NewGuid();
        var profile = InstructorProfile.Create(instructorId, "H", "B").Data!;
        _dbContext.Add(profile);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var notification = new DomainEventNotification<CourseEnrolledDomainEvent>(
            new CourseEnrolledDomainEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 50));

        await _handler.Handle(notification, CancellationToken.None);

        profile.CreditsEarned.Should().Be(0);
        _cacheService.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenProfileDoesNotExist_ReturnsEarly()
    {
        var instructorId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var price = CoursePrice.Create(50).Data!;
        var course = Course.Create(
            "Course", "Desc", "https://img.png", price, CourseLevel.Beginner, instructorId, "Instructor", categoryId).Data!;
        course.Id = courseId;
        _dbContext.Add(course);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var notification = new DomainEventNotification<CourseEnrolledDomainEvent>(
            new CourseEnrolledDomainEvent(Guid.NewGuid(), Guid.NewGuid(), courseId, 50));

        await _handler.Handle(notification, CancellationToken.None);

        _cacheService.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithValidEnrollment_AddsCreditsAndInvalidatesCache()
    {
        var instructorId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var profile = InstructorProfile.Create(instructorId, "H", "B").Data!;
        _dbContext.Add(profile);

        var price = CoursePrice.Create(50).Data!;
        var course = Course.Create(
            "Course", "Desc", "https://img.png", price, CourseLevel.Beginner, instructorId, "Instructor", categoryId).Data!;
        course.Id = courseId;
        _dbContext.Add(course);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var notification = new DomainEventNotification<CourseEnrolledDomainEvent>(
            new CourseEnrolledDomainEvent(Guid.NewGuid(), Guid.NewGuid(), courseId, 30));

        await _handler.Handle(notification, CancellationToken.None);

        profile.CreditsEarned.Should().Be(30);
        _cacheService.Verify(c => c.RemoveAsync($"instructor-full-profile-userid-{instructorId}", It.IsAny<CancellationToken>()), Times.Once);
    }
}
