using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skill_Loop.Domain.Entities.Support;

namespace Skill_Loop.Infrastructure.Persistence.Configurations;

public class SupportQuestionConfiguration : IEntityTypeConfiguration<SupportQuestion>
{
    public void Configure(EntityTypeBuilder<SupportQuestion> builder)
    {
        builder.ToTable("SupportQuestions");

        builder.HasKey(sq => sq.Id);

        builder.Property(sq => sq.Question)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(sq => sq.Answer)
            .HasMaxLength(5000);

        builder.Property(sq => sq.Category)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(sq => sq.UserEmail)
            .HasMaxLength(256);

        builder.Property(sq => sq.UserName)
            .HasMaxLength(100);

        builder.Property(sq => sq.AskedByUserId);

        builder.Property(sq => sq.IsPublished)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(sq => sq.IsAnswered)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(sq => sq.EmailSent)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(sq => sq.Category);
        builder.HasIndex(sq => sq.IsPublished);
        builder.HasIndex(sq => sq.IsAnswered);
        builder.HasIndex(sq => sq.AskedByUserId);
        builder.HasIndex(sq => sq.CreatedAt);
    }
}