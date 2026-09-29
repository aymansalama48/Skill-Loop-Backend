namespace Skill_Loop.UnitTests.Common;

using Microsoft.EntityFrameworkCore;
using Skill_Loop.Infrastructure.Persistence.Data;

/// <summary>
/// الـ InMemory provider مبيولّدش قيم لـ RowVersion، فبيconsidered كل تعديل على
/// UserWallet كـ concurrency conflict. بنشيل الـ concurrency tokens في بيئة الاختبار فقط.
/// </summary>
public sealed class InMemoryAppDbContext : AppDbContext
{
    public InMemoryAppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                property.IsConcurrencyToken = false;
                property.ValueGenerated = Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never;
            }
        }
    }
}
