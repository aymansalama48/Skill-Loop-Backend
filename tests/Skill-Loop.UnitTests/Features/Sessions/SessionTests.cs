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
}