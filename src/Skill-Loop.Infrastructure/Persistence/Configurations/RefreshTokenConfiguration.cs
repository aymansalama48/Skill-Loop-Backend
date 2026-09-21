using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skill_Loop.Infrastructure.Persistence.IdentityModels;

namespace Skill_Loop.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Token)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(rt => rt.UserId)
            .IsRequired();

        // فهرس فريد لمنع تكرار الـ Token نفسه
        builder.HasIndex(rt => rt.Token)
            .IsUnique()
            .HasDatabaseName("IX_RefreshTokens_Token");

        // فهرس لتسريع البحث عن Tokens الخاصة بمستخدم معين
        builder.HasIndex(rt => rt.UserId)
            .HasDatabaseName("IX_RefreshTokens_UserId");

        // العلاقة مع ApplicationUser
        builder.HasOne(rt => rt.User)
            .WithMany() // لو عاوز تعكس العلاقة من ApplicationUser لـ RefreshTokens
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
