using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skill_Loop.Domain.Entities.Instructors;

namespace Skill_Loop.Infrastructure.Persistence.Configurations;

internal sealed class InstructorReviewConfiguration : IEntityTypeConfiguration<InstructorReview>
{
    public void Configure(EntityTypeBuilder<InstructorReview> builder)
    {
        builder.ToTable("InstructorReviews");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Rating)
            .IsRequired();

        builder.Property(r => r.Comment)
            .HasMaxLength(1000);

        // علاقة 1 لمتعدد مع InstructorProfile
        builder.HasOne<InstructorProfile>()
            .WithMany(p => p.Reviews)
            .HasForeignKey(r => r.InstructorProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}