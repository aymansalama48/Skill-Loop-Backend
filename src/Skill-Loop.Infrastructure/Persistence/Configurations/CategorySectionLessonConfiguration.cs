using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skill_Loop.Domain.Entities.Courses;

namespace Skill_Loop.Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Slug)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(c => c.IconUrl)
            .HasMaxLength(1000);

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.HasIndex(c => c.Slug).IsUnique();
        builder.HasIndex(c => c.DisplayOrder);
    }
}

public sealed class SectionConfiguration : IEntityTypeConfiguration<Section>
{
    public void Configure(EntityTypeBuilder<Section> builder)
    {
        builder.ToTable("CourseSections");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasMany(s => s.Lessons)
            .WithOne()
            .HasForeignKey(l => l.SectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => new { s.CourseId, s.OrderIndex });
    }
}

public sealed class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> builder)
    {
        builder.ToTable("CourseLessons");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(l => l.VideoUrl)
            .HasColumnName("VideoUrl")
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(l => l.Duration)
            .HasColumnName("Duration")
            .IsRequired();

        builder.Property(l => l.StreamingResolution)
            .HasColumnName("StreamingResolution")
            .HasMaxLength(50);

        builder.Property(l => l.ExternalProviderId)
            .HasColumnName("ExternalProviderId")
            .HasMaxLength(150);

        // LessonMaterials
        builder.HasMany(l => l.Resources)
            .WithOne(m => m.Lesson)
            .HasForeignKey(m => m.LessonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(l => new { l.SectionId, l.OrderIndex });
    }
}
