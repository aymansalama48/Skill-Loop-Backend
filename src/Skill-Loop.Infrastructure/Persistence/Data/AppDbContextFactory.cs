using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Skill_Loop.Infrastructure.Persistence.Data;


/// <summary>
/// مصنع لإنشاء DbContext في وقت التصميم (لأوامر Migration)
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        optionsBuilder.UseSqlServer(
            "يوضع هنا ال connection String");

        return new AppDbContext(optionsBuilder.Options);
    }
}
