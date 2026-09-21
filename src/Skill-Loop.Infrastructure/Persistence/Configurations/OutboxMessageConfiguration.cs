using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skill_Loop.Infrastructure.Persistence.Outbox;

namespace Skill_Loop.Infrastructure.Persistence.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Content).IsRequired();

        // Index لسرعة الـ Query في الـ Background Job على الرسائل التي لم تُعالج بعد
        builder.HasIndex(x => new { x.ProcessedOnUtc, x.RetryCount });
    }
}