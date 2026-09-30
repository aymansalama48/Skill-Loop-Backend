using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skill_Loop.Domain.Entities.Promotions;

namespace Skill_Loop.Infrastructure.Persistence.Configurations.PromoCodes;

internal sealed class PromoCodeConfiguration : IEntityTypeConfiguration<PromoCode>
{
    public void Configure(EntityTypeBuilder<PromoCode> builder)
    {
        builder.ToTable("PromoCodes");

        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(p => p.Code)
            .IsUnique();

        builder.Property(p => p.DiscountType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.DiscountValue)
            .IsRequired();
            
        builder.Property(p => p.MaxRedemptions);
            
        builder.Property(p => p.RedemptionsCount)
            .IsRequired();
            
        builder.Property(p => p.IsActive)
            .IsRequired();

        builder.Property(p => p.RowVersion)
            .IsRowVersion();
    }
}
