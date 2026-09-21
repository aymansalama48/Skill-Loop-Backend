using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skill_Loop.Infrastructure.Persistence.IdentityModels;


namespace Skill_Loop.Infrastructure.Persistence.Configurations
{
    internal class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
    {
        public void Configure(EntityTypeBuilder<ApplicationRole> builder)
        {
            builder.Property(r => r.Description)
                   .HasMaxLength(500);
        }
    }
}
