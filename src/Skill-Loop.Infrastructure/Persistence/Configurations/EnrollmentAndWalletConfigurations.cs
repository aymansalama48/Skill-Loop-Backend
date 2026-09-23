using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skill_Loop.Domain.Entities.Enrollments;
using Skill_Loop.Domain.Entities.Wallets;
using Skill_Loop.Domain.Entities.Courses;

namespace Skill_Loop.Infrastructure.Persistence.Configurations;

public sealed class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("Enrollments");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ProgressPercentage)
            .HasPrecision(5, 2)
            .HasDefaultValue(0.0);

        builder.HasMany(e => e.LessonProgresses)
            .WithOne()
            .HasForeignKey(lp => lp.EnrollmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique constraint preventing duplicate active enrollments
        builder.HasIndex(e => new { e.UserId, e.CourseId }).IsUnique();
        builder.HasIndex(e => new { e.UserId, e.Status });
    }
}

public sealed class LessonProgressConfiguration : IEntityTypeConfiguration<LessonProgress>
{
    public void Configure(EntityTypeBuilder<LessonProgress> builder)
    {
        builder.ToTable("LessonProgresses");

        builder.HasKey(lp => lp.Id);

        builder.HasIndex(lp => new { lp.EnrollmentId, lp.LessonId }).IsUnique();
    }
}

public sealed class UserWalletConfiguration : IEntityTypeConfiguration<UserWallet>
{
    public void Configure(EntityTypeBuilder<UserWallet> builder)
    {
        builder.ToTable("UserWallets");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Balance)
            .IsRequired()
            .HasDefaultValue(0);

        // Optimistic concurrency control
        builder.Property(w => w.RowVersion)
            .IsRowVersion();

        builder.HasMany(w => w.Transactions)
            .WithOne()
            .HasForeignKey(t => t.WalletId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(w => w.UserId).IsUnique();
    }
}

public sealed class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
{
    public void Configure(EntityTypeBuilder<WalletTransaction> builder)
    {
        builder.ToTable("WalletTransactions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Description)
            .HasMaxLength(500);

        builder.HasIndex(t => new { t.WalletId, t.OccurredAt });
    }
}

public sealed class CourseBookmarkConfiguration : IEntityTypeConfiguration<CourseBookmark>
{
    public void Configure(EntityTypeBuilder<CourseBookmark> builder)
    {
        builder.ToTable("CourseBookmarks");

        builder.HasKey(b => b.Id);

        builder.HasOne(b => b.Course)
            .WithMany()
            .HasForeignKey(b => b.CourseId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(b => new { b.UserId, b.CourseId }).IsUnique();
    }
}

public sealed class CourseReviewConfiguration : IEntityTypeConfiguration<CourseReview>
{
    public void Configure(EntityTypeBuilder<CourseReview> builder)
    {
        builder.ToTable("CourseReviews");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Stars)
            .IsRequired();

        builder.Property(r => r.Comment)
            .HasMaxLength(1000);

        builder.HasIndex(r => new { r.CourseId, r.UserId }).IsUnique();
    }
}
