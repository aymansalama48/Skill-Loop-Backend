using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skill_Loop.Domain.Entities.Promotions;
using Skill_Loop.Domain.Entities.Wallets;

namespace Skill_Loop.Infrastructure.Persistence.Configurations;

public sealed class PromoCodeConfiguration : IEntityTypeConfiguration<PromoCode>
{
    public void Configure(EntityTypeBuilder<PromoCode> builder)
    {
        builder.ToTable("PromoCodes");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Code).IsRequired().HasMaxLength(50);
        builder.Property(p => p.DiscountType).HasConversion<int>();

        // Optimistic concurrency (يمنع تعدّي MaxRedemptions لو اتنين استخدموا الكود مع بعض)
        builder.Property(p => p.RowVersion).IsRowVersion();

        builder.HasIndex(p => p.Code).IsUnique();
    }
}

public sealed class PromoRedemptionConfiguration : IEntityTypeConfiguration<PromoRedemption>
{
    public void Configure(EntityTypeBuilder<PromoRedemption> builder)
    {
        builder.ToTable("PromoRedemptions");

        builder.HasKey(r => r.Id);

        // 🔒 كل مستخدم يستخدم الكود مرة واحدة بس
        builder.HasIndex(r => new { r.PromoCodeId, r.UserId }).IsUnique();
    }
}

public sealed class CreditPurchaseConfiguration : IEntityTypeConfiguration<CreditPurchase>
{
    public void Configure(EntityTypeBuilder<CreditPurchase> builder)
    {
        builder.ToTable("CreditPurchases");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Currency).IsRequired().HasMaxLength(3);
        builder.Property(p => p.Status).HasConversion<int>();
        builder.Property(p => p.GatewayReference).HasMaxLength(200);

        builder.HasIndex(p => new { p.UserId, p.CreatedAt });
        builder.HasIndex(p => p.GatewayReference);
    }
}
