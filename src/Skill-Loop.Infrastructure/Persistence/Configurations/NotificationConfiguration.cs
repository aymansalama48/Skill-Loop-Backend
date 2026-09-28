using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skill_Loop.Domain.Entities.Notifications;

namespace Skill_Loop.Infrastructure.Persistence.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Type).IsRequired().HasMaxLength(50);
        builder.Property(n => n.Title).IsRequired().HasMaxLength(Notification.TitleMaxLength);
        builder.Property(n => n.Body).HasMaxLength(Notification.BodyMaxLength);
        builder.Property(n => n.Data).HasMaxLength(1000);

        // 📈 أهم Index: "هات إشعارات اليوزر ده، الأحدث أول" و"عدّلي غير المقروء"
        builder.HasIndex(n => new { n.UserId, n.CreatedAt });
        builder.HasIndex(n => new { n.UserId, n.IsRead });
    }
}
