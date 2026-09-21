using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skill_Loop.Infrastructure.Persistence.IdentityModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Skill_Loop.Infrastructure.Persistence.Configurations
{
    internal class TbRolePermissionConfiguration : IEntityTypeConfiguration<TbRolePermission>
    {
        public void Configure(EntityTypeBuilder<TbRolePermission> builder)
        {
            builder.ToTable("TbRolePermissions");

            // المفتاح المركب (Composite Key) لمنع تكرار الصلاحية لنفس الدور
            builder.HasKey(rp => new { rp.RoleId, rp.PermissionId });

            // العلاقة مع ApplicationRole
            builder.HasOne(rp => rp.Role)
                   .WithMany(r => r.RolePermissions)
                   .HasForeignKey(rp => rp.RoleId)
                   .OnDelete(DeleteBehavior.Cascade);

            // العلاقة مع TbPermission
            builder.HasOne(rp => rp.Permission)
                   .WithMany(p => p.RolePermissions)
                   .HasForeignKey(rp => rp.PermissionId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
