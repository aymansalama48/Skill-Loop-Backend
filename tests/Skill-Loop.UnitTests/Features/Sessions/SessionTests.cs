namespace Skill_Loop.UnitTests.Features.Sessions;

using FluentAssertions;
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
        // استخدام دالة Create مع التوقيع الجديد (سعر، مدة، نوع)
        var result = Session.Create(Guid.NewGuid(), Guid.NewGuid(), "Test Title", 100, 60, SessionType.Online);

        result.IsSuccess.Should().BeTrue();
        result.Data.Status.Should().Be(SessionStatus.Draft);
    }

    [Fact]
    public void Session_Create_SetsTitleCorrectly()
    {
        var result = Session.Create(Guid.NewGuid(), Guid.NewGuid(), "My Session", 100, 60, SessionType.Online);

        result.IsSuccess.Should().BeTrue();
        result.Data.Title.Should().Be("My Session");
    }

    [Fact]
    public void Session_Create_SetsInstructorAndOwnerCorrectly()
    {
        var instructorId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        var result = Session.Create(instructorId, ownerId, "Test Title", 100, 60, SessionType.Online);

        result.IsSuccess.Should().BeTrue();
        result.Data.InstructorId.Should().Be(instructorId);
        result.Data.OwnerId.Should().Be(ownerId);
    }

    [Fact]
    public void Session_Create_InitializesEmptyMaterialsCollection()
    {
        var result = Session.Create(Guid.NewGuid(), Guid.NewGuid(), "Test Title", 100, 60, SessionType.Online);

        result.IsSuccess.Should().BeTrue();
        result.Data.Materials.Should().NotBeNull();
        result.Data.Materials.Should().BeEmpty();
    }

    [Fact]
    public void Session_DomainMethods_ModifyPropertiesCorrectly()
    {
        // Arrange
        var instructorId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var sessionResult = Session.Create(instructorId, ownerId, "Old Title", 100, 60, SessionType.Online);

        sessionResult.IsSuccess.Should().BeTrue();
        var session = sessionResult.Data;

        // Act - استخدام التوقيع الجديد لدالة UpdateDetails ودالة ChangeStatus
        session.UpdateDetails("New Test Session", 150, 45, SessionType.Offline);
        session.ChangeStatus(SessionStatus.Published);

        // Assert
        session.Title.Should().Be("New Test Session");
        session.PriceInCredits.Should().Be(150);
        session.DurationInMinutes.Should().Be(45);
        session.Type.Should().Be(SessionType.Offline);
        session.Status.Should().Be(SessionStatus.Published);
    }

    [Fact]
    public void Session_WithMaterials_CanAccessMaterials()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var sessionResult = Session.Create(Guid.NewGuid(), Guid.NewGuid(), "My Session", 100, 60, SessionType.Online);

        sessionResult.IsSuccess.Should().BeTrue();
        var session = sessionResult.Data;
        session.Id = sessionId;

        // Act - إنشاء الماتيريال بطريقة صحيحة وإضافته باستخدام دالة AddMaterial
        var material = SessionMaterial.Create(
            sessionId, "test.pdf", "application/pdf", 1024, "driveId123", null, 0, Guid.NewGuid());

        session.AddMaterial(material);

        // Assert
        session.Materials.Should().ContainSingle();
        session.Materials.First().Should().BeSameAs(material);
    }
}