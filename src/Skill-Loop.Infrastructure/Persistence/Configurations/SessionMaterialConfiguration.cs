using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skill_Loop.Domain.Entities.SessionMaterial;
using Skill_Loop.Domain.Enums;
using System;

namespace Skill_Loop.Infrastructure.Persistence.Configurations;

public sealed class SessionMaterialConfiguration : IEntityTypeConfiguration<SessionMaterial>
{
    public void Configure(EntityTypeBuilder<SessionMaterial> builder)
    {
        builder.ToTable("SessionMaterials");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SessionId)
            .IsRequired();

        builder.Property(x => x.FileName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.MimeType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.SizeBytes)
            .IsRequired();

        builder.Property(x => x.DriveFileId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.DriveFolderId)
            .HasMaxLength(256);

        builder.Property(x => x.SortOrder)
            .IsRequired();

        builder.Property(x => x.UploadedByUserId)
            .IsRequired();

        builder.Property(x => x.MaterialType)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(x => x.DriveFileId).IsUnique();
        builder.HasIndex(x => x.SessionId);

        builder.HasOne(x => x.Session)
            .WithMany(x => x.Materials)
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
