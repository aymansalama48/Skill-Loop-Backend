using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skill_Loop.Domain.Entities.Session;
using Skill_Loop.Domain.Enums;
using System;

namespace Skill_Loop.Infrastructure.Persistence.Configurations;

public sealed class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ToTable("Sessions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.InstructorId)
            .IsRequired();

        builder.Property(x => x.OwnerId)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(x => x.InstructorId);
        builder.HasIndex(x => x.OwnerId);
    }
}
