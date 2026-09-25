using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skill_Loop.Domain.Entities.Instructors;
using Skill_Loop.Infrastructure.Persistence.IdentityModels;

namespace Skill_Loop.Infrastructure.Persistence.Configurations;

internal sealed class InstructorProfileConfiguration : IEntityTypeConfiguration<InstructorProfile>
{
    public void Configure(EntityTypeBuilder<InstructorProfile> builder)
    {
        builder.ToTable("InstructorProfiles");

        builder.HasKey(p => p.Id);

        // علاقة 1-to-1 مع المستخدم
        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<InstructorProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(p => p.Headline)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Bio)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(p => p.Rating)
            .HasPrecision(3, 2) // مثلاً 4.50
            .HasDefaultValue(0.0);

        builder.Property(p => p.SessionsCompleted)
            .HasDefaultValue(0);

        builder.Property(p => p.CreditsEarned)
            .HasDefaultValue(0);

        builder.Property(p => p.IsApproved)
            .HasDefaultValue(false);
    }
}