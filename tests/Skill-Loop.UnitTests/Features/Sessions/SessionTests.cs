namespace Skill_Loop.UnitTests.Features.Sessions;

using FluentAssertions;
using Skill_Loop.Domain.Entities.Booking;
using Skill_Loop.Domain.Entities.Session;
using Skill_Loop.Domain.Entities.Sessions;
using Skill_Loop.Domain.Enums;
using System;
using System.Linq;
using Xunit;

public class SessionTests
{
    [Fact]
    public void Session_Create_SetsStatusToDraft()
    {
        // استخدام دالة Create بدلاً من new
        var session = Session.Create(Guid.NewGuid(), Guid.NewGuid(), "Test Title");

        session.Status.Should().Be(SessionStatus.Draft);
    }

    [Fact]
    public void Session_Create_SetsTitleCorrectly()
    {
        var session = Session.Create(Guid.NewGuid(), Guid.NewGuid(), "My Session");

        session.Title.Should().Be("My Session");
    }

    [Fact]
    public void Session_Create_SetsInstructorAndOwnerCorrectly()
    {
        var instructorId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        var session = Session.Create(instructorId, ownerId, "Test Title");

        session.InstructorId.Should().Be(instructorId);
        session.OwnerId.Should().Be(ownerId);
    }

    [Fact]
    public void Session_Create_InitializesEmptyMaterialsCollection()
    {
        var session = Session.Create(Guid.NewGuid(), Guid.NewGuid(), "Test Title");

        session.Materials.Should().NotBeNull();
        session.Materials.Should().BeEmpty();
    }

    [Fact]
    public void Session_DomainMethods_ModifyPropertiesCorrectly()
    {
        // Arrange
        var instructorId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var session = Session.Create(instructorId, ownerId, "Old Title");

        // Act - استخدام الدوال المساعدة بدلاً من التعيين المباشر
        session.UpdateDetails("New Test Session");
        session.ChangeStatus(SessionStatus.Published);

        // Assert
        session.Title.Should().Be("New Test Session");
        session.Status.Should().Be(SessionStatus.Published);
    }

    [Fact]
    public void Session_WithMaterials_CanAccessMaterials()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = Session.Create(Guid.NewGuid(), Guid.NewGuid(), "My Session");
        session.Id = sessionId;

        // Act - إنشاء الماتيريال بطريقة صحيحة وإضافته باستخدام دالة AddMaterial
        var material = SessionMaterial.Create(
            sessionId, "test.pdf", "application/pdf", 1024, "driveId123", null, 0, Guid.NewGuid());

        session.AddMaterial(material);

        // Assert
        session.Materials.Should().ContainSingle();
        session.Materials.First().Should().BeSameAs(material);
    }

    // ==========================================================
    // جدولة الجلسات (Live Session Slot)
    // ==========================================================

    [Fact]
    public void Session_Create_WithScheduling_SetsAllScheduleFields()
    {
        // Arrange
        var scheduledAt = DateTime.UtcNow.AddDays(3);

        // Act
        var session = Session.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Live C# Session",
            "A full description",
            scheduledAt,
            durationMinutes: 90,
            creditsPrice: 40,
            locationType: SessionLocationType.Offline,
            locationDetails: "Cairo, Nile Tower",
            maxParticipants: 5);

        // Assert
        session.Description.Should().Be("A full description");
        session.ScheduledAtUtc.Should().Be(scheduledAt);
        session.DurationMinutes.Should().Be(90);
        session.CreditsPrice.Should().Be(40);
        session.LocationType.Should().Be(SessionLocationType.Offline);
        session.LocationDetails.Should().Be("Cairo, Nile Tower");
        session.MaxParticipants.Should().Be(5);
    }

    [Fact]
    public void Session_Create_WithoutScheduling_UsesSafeDefaults()
    {
        var session = Session.Create(Guid.NewGuid(), Guid.NewGuid(), "Basic session");

        session.ScheduledAtUtc.Should().BeNull();
        session.DurationMinutes.Should().Be(60);
        session.CreditsPrice.Should().Be(0);
        session.LocationType.Should().Be(SessionLocationType.Online);
        session.MaxParticipants.Should().Be(1);
        session.Description.Should().BeNull();
    }

    [Fact]
    public void Session_UpdateSchedule_OnlyChangesProvidedValues()
    {
        // Arrange
        var original = DateTime.UtcNow.AddDays(2);
        var session = Session.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Session",
            description: "old",
            scheduledAtUtc: original,
            durationMinutes: 60,
            creditsPrice: 10,
            maxParticipants: 3);

        // Act — غيرPrix والمكان بس
        session.UpdateSchedule(creditsPrice: 25, maxParticipants: 8);

        // Assert — باقي الحقول زي ما هي
        session.CreditsPrice.Should().Be(25);
        session.MaxParticipants.Should().Be(8);
        session.ScheduledAtUtc.Should().Be(original);
        session.DurationMinutes.Should().Be(60);
    }

    [Theory]
    [InlineData(0, 60)]
    [InlineData(-30, 60)]
    [InlineData(10000, 480)]
    public void Session_UpdateSchedule_ClampsDuration(int duration, int expected)
    {
        var session = Session.Create(Guid.NewGuid(), Guid.NewGuid(), "Session");

        session.UpdateSchedule(durationMinutes: duration);

        session.DurationMinutes.Should().Be(expected);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-5, 1)]
    [InlineData(500, 50)]
    public void Session_UpdateSchedule_ClampsMaxParticipants(int participants, int expected)
    {
        var session = Session.Create(Guid.NewGuid(), Guid.NewGuid(), "Session");

        session.UpdateSchedule(maxParticipants: participants);

        session.MaxParticipants.Should().Be(expected);
    }

    [Fact]
    public void Session_UpdateDetails_WithNullDescription_KeepsExistingDescription()
    {
        var session = Session.Create(Guid.NewGuid(), Guid.NewGuid(), "Session", description: "keep me");

        session.UpdateDetails("New title");

        session.Title.Should().Be("New title");
        session.Description.Should().Be("keep me");
    }

    [Fact]
    public void Session_IsBookable_WhenPublishedScheduledInFutureAndHasSlots()
    {
        var session = Session.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Session",
            scheduledAtUtc: DateTime.UtcNow.AddDays(1),
            maxParticipants: 3);

        session.Publish();

        session.IsBookable(DateTime.UtcNow, activeBookingsCount: 1).Should().BeTrue();
        session.AvailableSlots(1).Should().Be(2);
    }

    [Fact]
    public void Session_IsBookable_IsFalseWhenDraft()
    {
        var session = Session.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Session",
            scheduledAtUtc: DateTime.UtcNow.AddDays(1),
            maxParticipants: 3);

        // لسه Draft
        session.IsBookable(DateTime.UtcNow, activeBookingsCount: 0).Should().BeFalse();
    }

    [Fact]
    public void Session_IsBookable_IsFalseWhenNoSlotsLeft()
    {
        var session = Session.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Session",
            scheduledAtUtc: DateTime.UtcNow.AddDays(1),
            maxParticipants: 2);

        session.Publish();

        session.IsBookable(DateTime.UtcNow, activeBookingsCount: 2).Should().BeFalse();
        session.AvailableSlots(2).Should().Be(0);
    }

    [Fact]
    public void Session_IsBookable_IsFalseWhenNoSchedule()
    {
        var session = Session.Create(Guid.NewGuid(), Guid.NewGuid(), "Session");
        session.Publish();

        session.IsBookable(DateTime.UtcNow, activeBookingsCount: 0).Should().BeFalse();
    }

    [Fact]
    public void Session_IsBookable_IsFalseWhenTimeAlreadyPassed()
    {
        var session = Session.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Session",
            scheduledAtUtc: DateTime.UtcNow.AddHours(-1),
            maxParticipants: 5);

        session.Publish();

        session.IsBookable(DateTime.UtcNow, activeBookingsCount: 0).Should().BeFalse();
    }

    [Fact]
    public void Session_EndsAtUtc_IsScheduledAtPlusDuration()
    {
        var scheduledAt = new DateTime(2026, 10, 1, 18, 0, 0, DateTimeKind.Utc);
        var session = Session.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Session",
            scheduledAtUtc: scheduledAt,
            durationMinutes: 90);

        session.EndsAtUtc.Should().Be(scheduledAt.AddMinutes(90));
    }

    [Fact]
    public void Session_Complete_SetsStatusToCompleted()
    {
        var session = Session.Create(Guid.NewGuid(), Guid.NewGuid(), "Session");
        session.Publish();

        var result = session.Complete();

        result.IsSuccess.Should().BeTrue();
        session.Status.Should().Be(SessionStatus.Completed);
    }

    [Fact]
    public void Session_Cancel_SetsStatusToCancelled()
    {
        var session = Session.Create(Guid.NewGuid(), Guid.NewGuid(), "Session");
        session.Publish();

        session.Cancel();

        session.Status.Should().Be(SessionStatus.Cancelled);
    }

    [Fact]
    public void Session_WithBookings_CanAccessBookings()
    {
        // Arrange
        var session = Session.Create(Guid.NewGuid(), Guid.NewGuid(), "Session", scheduledAtUtc: DateTime.UtcNow.AddDays(1));
        var booking = Booking.Create(session.Id, Guid.NewGuid(), 10).Data!;

        // Act
        session.AddBooking(booking);

        // Assert
        session.Bookings.Should().ContainSingle();
        session.Bookings.First().Should().BeSameAs(booking);

        // Remove
        session.RemoveBooking(booking);
        session.Bookings.Should().BeEmpty();
    }
}