using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skill_Loop.Domain.Entities.Sessions;

namespace Skill_Loop.Infrastructure.Persistence.Configurations;

public sealed class SessionReviewConfiguration : IEntityTypeConfiguration<SessionReview>
{
    public void Configure(EntityTypeBuilder<SessionReview> builder)
    {
        builder.ToTable("SessionReviews");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SessionId)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Stars)
            .IsRequired();

        builder.Property(x => x.Comment)
            .HasMaxLength(2000);

        builder.HasIndex(x => new { x.SessionId, x.UserId }).IsUnique();
    }
}
