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

        // SHA-256 produces 32 bytes -> 44 Base64 characters. Bounded so the value cannot
        // be silently truncated by a misconfigured column and start colliding.
        builder.Property(rt => rt.TokenHash)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(rt => rt.ReplacedByTokenHash)
            .HasMaxLength(64);

        builder.Property(rt => rt.UserId)
            .IsRequired();

        builder.Property(rt => rt.TokenFamilyId)
            .IsRequired();

        // Lookup path: hash the presented token and find the matching row. The unique
        // index is what makes that a single seek instead of a table scan, and it also
        // guarantees a digest can never map to two rows (which would make
        // RevokeFamilyAsync ambiguous).
        builder.HasIndex(rt => rt.TokenHash)
            .IsUnique()
            .HasDatabaseName("IX_RefreshTokens_TokenHash");

        // Reuse detection revokes a whole family in one UPDATE.
        builder.HasIndex(rt => rt.TokenFamilyId)
            .HasDatabaseName("IX_RefreshTokens_TokenFamilyId");

        // فهرس لتسريع البحث عن Tokens الخاصة بمستخدم معين
        builder.HasIndex(rt => rt.UserId)
            .HasDatabaseName("IX_RefreshTokens_UserId");

        // Relationship with ApplicationUser
        builder.HasOne(rt => rt.User)
            .WithMany() // لو عاوز تعكس العلاقة من ApplicationUser لـ RefreshTokens
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
