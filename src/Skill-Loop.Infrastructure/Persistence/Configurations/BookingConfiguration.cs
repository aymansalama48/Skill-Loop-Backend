using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skill_Loop.Domain.Entities.Booking;
using Skill_Loop.Domain.Entities.Sessions;
using System;

namespace Skill_Loop.Infrastructure.Persistence.Configurations;

public sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SessionId)
            .IsRequired();

        builder.Property(x => x.LearnerUserId)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.PriceInCredits)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.ScheduledAtUtc);

        builder.Property(x => x.BookedAtUtc)
            .IsRequired();

        builder.Property(x => x.CancellationReason)
            .HasMaxLength(500);

        builder.HasOne<Session>()
            .WithMany(x => x.Bookings)
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.SessionId);
        builder.HasIndex(x => x.LearnerUserId);
        builder.HasIndex(x => new { x.SessionId, x.LearnerUserId, x.Status });

        // متعلم واحد ميقدرش يحجز نفس الجلسة مرتين في نفس الوقت
        // (الحجوزات الملغاة/المرفوضة مش بتعتبر نشطة فممكن يعيد الحجز)
        builder.HasIndex(x => new { x.SessionId, x.LearnerUserId })
            .IsUnique()
            .HasFilter("[Status] IN ('Pending', 'Confirmed', 'InProgress', 'Completed')");
    }
}
