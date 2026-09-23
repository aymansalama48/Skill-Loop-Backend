namespace Skill_Loop.UnitTests.Features.Sessions;

using System;
using System.Linq;
using FluentAssertions;
using Skill_Loop.Domain.Entities.SessionMaterial;
using Skill_Loop.Domain.Entities.SessionMaterial.Events;
using Skill_Loop.Domain.Enums;
using Skill_Loop.Domain.Common.Events;
using Xunit;

public class SessionMaterialTests
{
    [Fact]
    public void Create_SetsAllPropertiesCorrectly()
    {
        var sessionId = Guid.NewGuid();
        var uploadedBy = Guid.NewGuid();

        var material = SessionMaterial.Create(
            sessionId: sessionId,
            fileName: "lecture.pdf",
            mimeType: "application/pdf",
            sizeBytes: 102400,
            driveFileId: "file-123",
            driveFolderId: "folder-456",
            sortOrder: 1,
            uploadedByUserId: uploadedBy,
            materialType: MaterialType.Document);

        material.SessionId.Should().Be(sessionId);
        material.FileName.Should().Be("lecture.pdf");
        material.MimeType.Should().Be("application/pdf");
        material.SizeBytes.Should().Be(102400);
        material.DriveFileId.Should().Be("file-123");
        material.DriveFolderId.Should().Be("folder-456");
        material.SortOrder.Should().Be(1);
        material.UploadedByUserId.Should().Be(uploadedBy);
        material.MaterialType.Should().Be(MaterialType.Document);
    }

    [Fact]
    public void Create_WithNullMaterialType_NullablePropertyIsNull()
    {
        var material = SessionMaterial.Create(
            Guid.NewGuid(), "test.txt", "text/plain", 100,
            "file-1", null, 0, Guid.NewGuid());

        material.MaterialType.Should().BeNull();
    }

    [Fact]
    public void Create_DefaultDriveFolderId_IsNull()
    {
        var material = SessionMaterial.Create(
            Guid.NewGuid(), "test.txt", "text/plain", 100,
            "file-1", null, 0, Guid.NewGuid());

        material.DriveFolderId.Should().BeNull();
    }

    [Fact]
    public void Create_AddsSessionMaterialUploadedEvent()
    {
        var material = SessionMaterial.Create(
            Guid.NewGuid(), "test.txt", "text/plain", 100,
            "file-1", null, 0, Guid.NewGuid());

        material.DomainEvents.Should().NotBeEmpty();
        material.DomainEvents.Should().Contain(e => e is SessionMaterialUploadedEvent);
    }

    [Fact]
    public void Create_DomainEvent_ContainsSameMaterial()
    {
        var material = SessionMaterial.Create(
            Guid.NewGuid(), "test.txt", "text/plain", 100,
            "file-1", null, 0, Guid.NewGuid());

        var domainEvent = material.DomainEvents.OfType<SessionMaterialUploadedEvent>().Single();
        domainEvent.Material.Should().BeSameAs(material);
    }

    [Fact]
    public void Create_MultipleMaterials_HaveDifferentInstances()
    {
        var material1 = SessionMaterial.Create(
            Guid.NewGuid(), "a.txt", "text/plain", 10, "f1", null, 0, Guid.NewGuid());
        var material2 = SessionMaterial.Create(
            Guid.NewGuid(), "b.pdf", "application/pdf", 20, "f2", null, 1, Guid.NewGuid());

        material1.Should().NotBeSameAs(material2);
        material1.FileName.Should().Be("a.txt");
        material2.FileName.Should().Be("b.pdf");
    }
}
