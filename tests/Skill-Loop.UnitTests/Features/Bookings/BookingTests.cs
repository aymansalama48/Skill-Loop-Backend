namespace Skill_Loop.UnitTests.Features.Bookings;

using FluentAssertions;
using Skill_Loop.Domain.Entities.Booking;
using Skill_Loop.Domain.Entities.Booking.Events;
using Skill_Loop.Domain.Entities.Sessions.Events;
using Skill_Loop.Domain.Enums;
using System;
using System.Linq;
using Xunit;

/// <summary>
/// اختبارات الـ Domain للـ Booking (دورة الحياة + الأحداث)
/// </summary>
public class BookingTests
{
    [Fact]
    public void Create_WithValidData_SetsStatusToConfirmedAndRaisesCreatedEvent()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var learnerId = Guid.NewGuid();
        var scheduledAt = DateTime.UtcNow.AddDays(1);

        // Act
        var result = Booking.Create(sessionId, learnerId, 25, scheduledAt);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var booking = result.Data!;
        booking.SessionId.Should().Be(sessionId);
        booking.LearnerUserId.Should().Be(learnerId);
        booking.PriceInCredits.Should().Be(25);
        booking.ScheduledAtUtc.Should().Be(scheduledAt);
        booking.Status.Should().Be(BookingStatus.Confirmed);
        booking.DomainEvents.Should().ContainSingle(e => e is BookingCreatedDomainEvent);
    }

    [Fact]
    public void Create_WithEmptySessionId_ReturnsValidationFailure()
    {
        var result = Booking.Create(Guid.Empty, Guid.NewGuid(), 10);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "BOOKING_SESSION_ID_REQUIRED");
    }

    [Fact]
    public void Create_WithEmptyLearnerId_ReturnsValidationFailure()
    {
        var result = Booking.Create(Guid.NewGuid(), Guid.Empty, 10);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "BOOKING_LEARNER_ID_REQUIRED");
    }

    [Fact]
    public void Create_WithNegativePrice_ReturnsValidationFailure()
    {
        var result = Booking.Create(Guid.NewGuid(), Guid.NewGuid(), -5);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "BOOKING_INVALID_PRICE");
    }

    [Fact]
    public void Start_FromConfirmed_SetsStatusToInProgress()
    {
        var booking = Booking.Create(Guid.NewGuid(), Guid.NewGuid(), 10).Data!;

        var result = booking.Start();

        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(BookingStatus.InProgress);
        booking.StartedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void Start_FromCompleted_ReturnsConflictFailure()
    {
        var booking = Booking.Create(Guid.NewGuid(), Guid.NewGuid(), 10).Data!;
        booking.Complete(Guid.NewGuid());

        var result = booking.Start();

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "BOOKING_INVALID_STATUS_TRANSITION");
    }

    [Fact]
    public void Complete_RaisesSessionCompletedDomainEventWithInstructorAndPrice()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var learnerId = Guid.NewGuid();
        var instructorId = Guid.NewGuid();
        var booking = Booking.Create(sessionId, learnerId, 30).Data!;

        // Act
        var result = booking.Complete(instructorId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(BookingStatus.Completed);
        booking.CompletedAtUtc.Should().NotBeNull();

        var sessionCompleted = booking.DomainEvents
            .OfType<SessionCompletedDomainEvent>()
            .Should()
            .ContainSingle()
            .Subject;

        sessionCompleted.SessionId.Should().Be(sessionId);
        sessionCompleted.InstructorId.Should().Be(instructorId);
        sessionCompleted.LearnerUserId.Should().Be(learnerId);
        sessionCompleted.PriceInCredits.Should().Be(30);
    }

    [Fact]
    public void Cancel_FromConfirmed_SetsCancelledAndRaisesRefundableEvent()
    {
        var booking = Booking.Create(Guid.NewGuid(), Guid.NewGuid(), 40).Data!;

        var result = booking.Cancel("Learner changed their mind");

        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(BookingStatus.Cancelled);
        booking.CancellationReason.Should().Be("Learner changed their mind");
        booking.CancelledAtUtc.Should().NotBeNull();
        booking.IsRefundable.Should().BeFalse();

        var cancelled = booking.DomainEvents
            .OfType<BookingCancelledDomainEvent>()
            .Should()
            .ContainSingle()
            .Subject;

        cancelled.Refundable.Should().BeTrue();
        cancelled.RefundAmountInCredits.Should().Be(40);
    }

    [Fact]
    public void Cancel_FromInProgress_ReturnsConflictFailure()
    {
        var booking = Booking.Create(Guid.NewGuid(), Guid.NewGuid(), 10).Data!;
        booking.Start();

        var result = booking.Cancel();

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "BOOKING_INVALID_STATUS_TRANSITION");
    }

    [Fact]
    public void Reject_FromConfirmed_SetsRejectedAndIsStillRefundable()
    {
        var booking = Booking.Create(Guid.NewGuid(), Guid.NewGuid(), 15).Data!;

        var result = booking.Reject("Not available anymore");

        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(BookingStatus.Rejected);

        var cancelled = booking.DomainEvents
            .OfType<BookingCancelledDomainEvent>()
            .Should()
            .ContainSingle()
            .Subject;

        cancelled.Refundable.Should().BeTrue();
        cancelled.Reason.Should().Be("Not available anymore");
    }

    [Fact]
    public void MarkNoShow_FromConfirmed_SetsNoShowWithoutRefund()
    {
        var booking = Booking.Create(Guid.NewGuid(), Guid.NewGuid(), 20).Data!;

        var result = booking.MarkNoShow();

        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(BookingStatus.NoShow);
        booking.DomainEvents.Should().NotContain(e => e is BookingCancelledDomainEvent);
    }

    [Theory]
    [InlineData(BookingStatus.Pending, true)]
    [InlineData(BookingStatus.Confirmed, true)]
    [InlineData(BookingStatus.InProgress, true)]
    [InlineData(BookingStatus.Completed, true)]
    [InlineData(BookingStatus.Cancelled, false)]
    [InlineData(BookingStatus.Rejected, false)]
    [InlineData(BookingStatus.NoShow, false)]
    public void IsActiveStatus_ClassifiesStatusesCorrectly(BookingStatus status, bool expected)
    {
        Booking.IsActiveStatus(status).Should().Be(expected);
    }
}
