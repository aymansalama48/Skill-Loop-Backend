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

        // Value Object: VideoResource
        builder.OwnsOne(l => l.Video, video =>
        {
            video.Property(v => v.VideoUrl)
                .HasColumnName("VideoUrl")
                .IsRequired()
                .HasMaxLength(1000);

            video.Property(v => v.Duration)
                .HasColumnName("Duration")
                .IsRequired();

            video.Property(v => v.StreamingResolution)
                .HasColumnName("StreamingResolution")
                .HasMaxLength(50);

            video.Property(v => v.ExternalProviderId)
                .HasColumnName("ExternalProviderId")
                .HasMaxLength(150);
        });

        // Value Object collection: Resources
        builder.OwnsMany(l => l.Resources, resource =>
        {
            resource.ToTable("LessonResources");
            resource.WithOwner().HasForeignKey("LessonId");
            resource.Property<Guid>("Id").ValueGeneratedOnAdd();
            resource.HasKey("Id");
            resource.Property(r => r.FileName).HasMaxLength(256).IsRequired();
            resource.Property(r => r.StorageUrl).HasMaxLength(1000).IsRequired();
        });

        builder.HasIndex(l => new { l.SectionId, l.OrderIndex });
    }
}
