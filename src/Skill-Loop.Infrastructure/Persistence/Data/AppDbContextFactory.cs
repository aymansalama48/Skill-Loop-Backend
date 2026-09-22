using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Skill_Loop.Infrastructure.Persistence.Data;

/// <summary>
/// مصنع لإنشاء DbContext في وقت التصميم (لأوامر Migration)
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "Skill-Loop.Api");
        if (!Directory.Exists(basePath))
        {
            basePath = Directory.GetCurrentDirectory();
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=AYMAN\\MSSQLSERVER01;Database=Skill-Loop;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Connect Timeout=30;Encrypt=True;TrustServerCertificate=True;Packet Size=4096;Command Timeout=0";

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));

        return new AppDbContext(optionsBuilder.Options);
    }
}
