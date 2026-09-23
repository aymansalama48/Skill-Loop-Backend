using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skill_Loop.Domain.Entities.SiteSettings;

namespace Skill_Loop.Infrastructure.Persistence.Configurations;

internal sealed class SiteSettingsConfiguration : IEntityTypeConfiguration<SiteSettings>
{
    public void Configure(EntityTypeBuilder<SiteSettings> builder)
    {
        // 1. تحديد اسم الجدول
        builder.ToTable("SiteSettings");

        // 2. تحديد المفتاح الأساسي (بافتراض إن BaseEntity بيحتوي على Id)
        builder.HasKey(x => x.Id);

        // 3. ضبط القيود (Max Lengths) لتوفير مساحة قاعدة البيانات وتحسين الأداء

        builder.Property(x => x.AppName)
            .HasMaxLength(100)
            .IsRequired(false); // لأنها nullable في الكيان (?)

        builder.Property(x => x.LogoName)
            .HasMaxLength(255)
            .IsRequired(false);

        builder.Property(x => x.SupportEmail)
            .HasMaxLength(150)
            .IsRequired(false);

        builder.Property(x => x.ContactPhoneNumber)
            .HasMaxLength(30)
            .IsRequired(false);

        builder.Property(x => x.WhatsAppNumber)
            .HasMaxLength(30)
            .IsRequired(false);

        builder.Property(x => x.Address)
            .HasMaxLength(500)
            .IsRequired(false);

        // روابط السوشيال ميديا والموقع (يفضل مساحة أكبر شوية تحسباً للروابط الطويلة)
        builder.Property(x => x.WebsiteUrl)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(x => x.FacebookUrl)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(x => x.InstagramUrl)
            .HasMaxLength(500)
            .IsRequired(false);
    }
}