using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skill_Loop.Domain.Entities.Emails;

namespace Skill_Loop.Infrastructure.Persistence.Configurations.Emails;

internal sealed class EmailLogConfiguration : IEntityTypeConfiguration<EmailLog>
{
    public void Configure(EntityTypeBuilder<EmailLog> builder)
    {
        builder.ToTable("EmailLogs");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.Type)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.RecipientEmail)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.ReferenceId)
            .HasMaxLength(100);

        builder.Property(e => e.Subject)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.Body)
            .IsRequired(); // usually long string (nvarchar(max))

        builder.Property(e => e.Status)
            .IsRequired();

        builder.Property(e => e.Attempts)
            .IsRequired();

        builder.Property(e => e.LastError)
            .HasMaxLength(4000);

        builder.Property(e => e.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(e => new { e.Type, e.ReferenceId, e.RecipientEmail })
            .IsUnique()
            .HasFilter("[ReferenceId] IS NOT NULL"); // Idempotency key
    }
}
