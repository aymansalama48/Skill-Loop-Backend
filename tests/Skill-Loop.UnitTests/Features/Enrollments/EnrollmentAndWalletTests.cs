using FluentAssertions;
using Skill_Loop.Domain.Entities.Enrollments;
using Skill_Loop.Domain.Entities.Enrollments.Events;
using Skill_Loop.Domain.Entities.Wallets;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.UnitTests.Features.Enrollments;

public class EnrollmentAndWalletTests
{
    [Fact]
    public void DeductCredits_WithSufficientBalance_ShouldDeductAndRecordTransaction()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var wallet = UserWallet.Create(userId, initialBalance: 100);
        var courseId = Guid.NewGuid();

        // Act
        var result = wallet.DeductCredits(40, courseId, "Enrolled in course");

        // Assert
        result.IsSuccess.Should().BeTrue();
        wallet.Balance.Should().Be(60);
        wallet.Transactions.Should().HaveCount(1);
        wallet.Transactions.First().Amount.Should().Be(40);
        wallet.Transactions.First().Type.Should().Be(TransactionType.CreditDeduction);
    }

    [Fact]
    public void DeductCredits_WithInsufficientBalance_ShouldFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var wallet = UserWallet.Create(userId, initialBalance: 30);
        var courseId = Guid.NewGuid();

        // Act
        var result = wallet.DeductCredits(50, courseId, "Enrolled in course");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "Wallet.InsufficientBalance");
        wallet.Balance.Should().Be(30);
    }

    [Fact]
    public void MarkLessonCompleted_ShouldUpdateProgressPercentage()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var enrollment = Enrollment.Create(userId, courseId, creditsPaid: 50, totalCourseLessons: 4).Data!;
        var lesson1Id = Guid.NewGuid();
        var lesson2Id = Guid.NewGuid();

        // Act
        enrollment.MarkLessonCompleted(lesson1Id, totalCourseLessons: 4);
        enrollment.ProgressPercentage.Should().Be(25.0);
        enrollment.Status.Should().Be(EnrollmentStatus.Active);

        enrollment.MarkLessonCompleted(lesson2Id, totalCourseLessons: 4);
        enrollment.ProgressPercentage.Should().Be(50.0);
        enrollment.LastWatchedLessonId.Should().Be(lesson2Id);
    }

    [Fact]
    public void MarkAllLessonsCompleted_ShouldCompleteEnrollment()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var enrollment = Enrollment.Create(userId, courseId, creditsPaid: 0, totalCourseLessons: 1).Data!;
        var lessonId = Guid.NewGuid();

        // Act
        enrollment.MarkLessonCompleted(lessonId, totalCourseLessons: 1);

        // Assert
        enrollment.ProgressPercentage.Should().Be(100.0);
        enrollment.Status.Should().Be(EnrollmentStatus.Completed);
        enrollment.DomainEvents.Should().ContainSingle(e => e is CourseCompletedDomainEvent);
    }
}
