using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skill_Loop.Domain.Entities.Courses;

namespace Skill_Loop.Infrastructure.Persistence.Configurations;

public sealed class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Courses");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Description)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(c => c.ThumbnailUrl)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(c => c.InstructorName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(c => c.Credits)
            .HasColumnName("Credits")
            .IsRequired();

        builder.Property(c => c.AverageRating)
            .HasColumnName("AverageRating")
            .HasPrecision(3, 2)
            .HasDefaultValue(0.0);

        builder.Property(c => c.TotalReviews)
            .HasColumnName("TotalReviews")
            .HasDefaultValue(0);

        // CourseMaterials
        builder.HasMany(c => c.Attachments)
            .WithOne(m => m.Course)
            .HasForeignKey(m => m.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationships
        builder.HasOne(c => c.Category)
            .WithMany(cat => cat.Courses)
            .HasForeignKey(c => c.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Sections)
            .WithOne()
            .HasForeignKey(s => s.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Reviews)
            .WithOne()
            .HasForeignKey(r => r.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for high performance search and filtering
        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.CategoryId);
        builder.HasIndex(c => c.InstructorId);
        builder.HasIndex(c => new { c.Status, c.CategoryId, c.Level });
        builder.HasIndex(c => c.Title);
    }
}
