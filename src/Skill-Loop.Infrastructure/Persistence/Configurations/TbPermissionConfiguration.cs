using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skill_Loop.Infrastructure.Persistence.IdentityModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Skill_Loop.Infrastructure.Persistence.Configurations
{
    internal class TbPermissionConfiguration : IEntityTypeConfiguration<TbPermission>
    {
        public void Configure(EntityTypeBuilder<TbPermission> builder)
        {
            builder.ToTable("TbPermissions");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                   .IsRequired()
                   .HasMaxLength(150);

            // ضمان عدم تكرار كود الصلاحية
            builder.HasIndex(p => p.Name)
                   .IsUnique();

            builder.Property(p => p.DisplayName)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(p => p.Module)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(p => p.Description)
                   .HasMaxLength(500);

            builder.HasIndex(p => p.Module);
        }
    }
}
