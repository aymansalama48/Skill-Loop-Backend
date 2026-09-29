using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skill_Loop.Domain.Entities.Booking;
using Skill_Loop.Domain.Entities.Instructors;

namespace Skill_Loop.Infrastructure.Persistence.Configurations;

internal sealed class InstructorAvailabilityConfiguration : IEntityTypeConfiguration<InstructorAvailability>
{
    public void Configure(EntityTypeBuilder<InstructorAvailability> builder)
    {
        builder.ToTable("InstructorAvailabilities");

        builder.HasKey(x => x.Id);

        // علاقة 1 لمتعدد مع البروفايل
        builder.HasOne<InstructorProfile>()
            .WithMany(p => p.Availabilities)
            .HasForeignKey(x => x.InstructorProfileId)
            .OnDelete(DeleteBehavior.Cascade); // إذا تم حذف البروفايل، تُحذف المواعيد
    }
}