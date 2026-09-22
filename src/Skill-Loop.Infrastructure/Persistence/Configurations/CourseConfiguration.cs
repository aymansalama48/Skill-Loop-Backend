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

        // Value Object: Price
        builder.OwnsOne(c => c.Price, price =>
        {
            price.Property(p => p.Credits)
                .HasColumnName("Credits")
                .IsRequired();
        });

        // Value Object: Rating
        builder.OwnsOne(c => c.Rating, rating =>
        {
            rating.Property(r => r.AverageRating)
                .HasColumnName("AverageRating")
                .HasPrecision(3, 2)
                .HasDefaultValue(0.0);

            rating.Property(r => r.TotalReviews)
                .HasColumnName("TotalReviews")
                .HasDefaultValue(0);
        });

        // Value Object collection: Attachments (Stored as JSON / owned)
        builder.OwnsMany(c => c.Attachments, attachment =>
        {
            attachment.ToTable("CourseAttachments");
            attachment.WithOwner().HasForeignKey("CourseId");
            attachment.Property<Guid>("Id").ValueGeneratedOnAdd();
            attachment.HasKey("Id");
            attachment.Property(a => a.FileName).HasMaxLength(256).IsRequired();
            attachment.Property(a => a.StorageUrl).HasMaxLength(1000).IsRequired();
        });

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
