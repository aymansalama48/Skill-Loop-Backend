using Microsoft.EntityFrameworkCore;
using Skill_Loop.Domain.Common.Entities;
using System.Reflection;

namespace Skill_Loop.Infrastructure.Persistence.Data;

public static class ModelBuilderExtensions
{
    public static void ApplySoftDeleteGlobalFilters(this ModelBuilder builder)
    {
        // 1. نجيب كل الجداول (Entities) اللي مسجلة في الـ DbContext واللي بتورث من SoftDeleteEntity
        var entityTypes = builder.Model.GetEntityTypes()
            .Where(e => typeof(SoftDeleteEntity).IsAssignableFrom(e.ClrType));

        foreach (var entityType in entityTypes)
        {
            // 2. نستدعي الدالة الـ Generic (SetSoftDeleteFilter) لكل جدول
            var method = typeof(ModelBuilderExtensions)
                .GetMethod(nameof(SetSoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static)?
                .MakeGenericMethod(entityType.ClrType);

            method?.Invoke(null, new object[] { builder });
        }
    }

    // الدالة دي هي اللي بتعمل الفلتر بطريقة Strongly-Typed وآمنة
    private static void SetSoftDeleteFilter<T>(ModelBuilder builder) where T : SoftDeleteEntity
    {
        // الفلتر السحري: أي Query هيشتغل على الجدول ده، هيجيب اللي IsDeleted بتاعه بـ false بس
        builder.Entity<T>().HasQueryFilter(x => !x.IsDeleted);
    }
}
