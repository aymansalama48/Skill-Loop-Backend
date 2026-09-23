namespace Skill_Loop.UnitTests.Features.Sessions;

using System;
using FluentAssertions;
using Skill_Loop.Domain.Entities.Session;
using Skill_Loop.Domain.Entities.SessionMaterial;
using Skill_Loop.Domain.Enums;
using Xunit;

public class SessionTests
{
    [Fact]
    public void Session_DefaultStatus_IsDraft()
    {
        var session = new Session();
        session.Status.Should().Be(SessionStatus.Draft);
    }

    [Fact]
    public void Session_DefaultTitle_IsEmpty()
    {
        var session = new Session();
        session.Title.Should().Be(string.Empty);
    }

    [Fact]
    public void Session_DefaultInstructorAndOwner_IsEmptyGuid()
    {
        var session = new Session();
        session.InstructorId.Should().Be(Guid.Empty);
        session.OwnerId.Should().Be(Guid.Empty);
    }

    [Fact]
    public void Session_Materials_IsEmptyCollection()
    {
        var session = new Session();
        session.Materials.Should().NotBeNull();
        session.Materials.Should().BeEmpty();
    }

    [Fact]
    public void Session_SetProperties_StoresCorrectly()
    {
        var instructorId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var session = new Session
        {
            Title = "Test Session",
            InstructorId = instructorId,
            OwnerId = ownerId,
            Status = SessionStatus.Published,
        };

        session.Title.Should().Be("Test Session");
        session.InstructorId.Should().Be(instructorId);
        session.OwnerId.Should().Be(ownerId);
        session.Status.Should().Be(SessionStatus.Published);
    }

    [Fact]
    public void Session_WithMaterials_CanAccessMaterials()
    {
        var sessionId = Guid.NewGuid();
        var session = new Session { Id = sessionId, Title = "My Session" };

        var material = new SessionMaterial();
        session.Materials.Add(material);

        session.Materials.Should().ContainSingle();
        session.Materials.First().Should().BeSameAs(material);
    }
}
