using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skill_Loop.Domain.Entities.Sessions;
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

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.Property(x => x.ScheduledAtUtc);

        builder.Property(x => x.DurationMinutes)
            .IsRequired()
            .HasDefaultValue(Session.DefaultDurationMinutes);

        builder.Property(x => x.CreditsPrice)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.LocationType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue(SessionLocationType.Online);

        builder.Property(x => x.LocationDetails)
            .HasMaxLength(500);

        builder.Property(x => x.MaxParticipants)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Ignore(x => x.EndsAtUtc);

        builder.HasIndex(x => x.InstructorId);
        builder.HasIndex(x => x.OwnerId);
        builder.HasIndex(x => x.ScheduledAtUtc);
        builder.HasIndex(x => new { x.Status, x.ScheduledAtUtc });
    }
}
