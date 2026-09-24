using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skill_Loop.Domain.Entities.Chat;

namespace Skill_Loop.Infrastructure.Persistence.Configurations;

public sealed class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("Conversations");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.LastMessagePreview)
            .HasMaxLength(Conversation.PreviewMaxLength);

        // 🔒 المحادثة بين نفس الشخصين لازم تكون صف واحد بس (الترتيب ثابت من الـ Domain)
        builder.HasIndex(c => new { c.ParticipantOneId, c.ParticipantTwoId }).IsUnique();

        // عشان "محادثاتي" تدور بسرعة لو أنا الطرف التاني
        builder.HasIndex(c => c.ParticipantTwoId);
    }
}

public sealed class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("ChatMessages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Content)
            .IsRequired()
            .HasMaxLength(ChatMessage.MaxContentLength);

        // لو اتمسحت محادثة، رسايلها تتمسح معاها
        builder.HasOne<Conversation>()
            .WithMany()
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        // 📈 أهم Index: تحميل رسايل محادثة مرتبة بالوقت
        builder.HasIndex(m => new { m.ConversationId, m.SentAt });
    }
}
