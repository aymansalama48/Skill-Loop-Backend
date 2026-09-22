using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Entities.Invitation;
using Skill_Loop.Domain.Entities.OtpVerification;
using System.Numerics;
using static System.Net.Mime.MediaTypeNames;

namespace Skill_Loop.Infrastructure.Persistence.Data;

/// <summary>
/// هذا الكلاس يعمل كمترجم (Adapter) بين قاعدة بيانات EF Core وبين طبقة الـ Application.
/// يخفي تفاصيل الـ DbSet ويوفر فقط IQueryable.
/// </summary>
public class ApplicationDbContextAdapter(AppDbContext context) : IApplicationDbContext
{
    // ==============================
    // 1. القراءة (IQueryable Mapping)
    // ==============================




    public IQueryable<OtpVerification> OtpVerifications => context.OtpVerifications;
    public IQueryable<StaffInvitation> StaffInvitations => context.StaffInvitations;
    public IQueryable<Skill_Loop.Domain.Entities.Courses.Course> Courses => context.Courses;
    public IQueryable<Skill_Loop.Domain.Entities.Courses.Category> Categories => context.Categories;
    public IQueryable<Skill_Loop.Domain.Entities.Enrollments.Enrollment> Enrollments => context.Enrollments;
    public IQueryable<Skill_Loop.Domain.Entities.Wallets.UserWallet> UserWallets => context.UserWallets;
    public IQueryable<Skill_Loop.Domain.Entities.Courses.CourseBookmark> CourseBookmarks => context.CourseBookmarks;
    public IQueryable<Skill_Loop.Domain.Entities.Courses.CourseReview> CourseReviews => context.CourseReviews;

    // ==============================
    // عمليات الكتابة (عنصر واحد)
    // ==============================
    public void Add<TEntity>(TEntity entity) where TEntity : class
    {
        context.Set<TEntity>().Add(entity);
    }

    public void Update<TEntity>(TEntity entity) where TEntity : class
    {
        context.Set<TEntity>().Update(entity);
    }

    public void Remove<TEntity>(TEntity entity) where TEntity : class
    {
        context.Set<TEntity>().Remove(entity);
    }

    // ==============================
    // 👇 عمليات الكتابة (مجموعة Range)
    // ==============================
    public void AddRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
    {
        context.Set<TEntity>().AddRange(entities);
    }

    public void UpdateRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
    {
        context.Set<TEntity>().UpdateRange(entities);
    }

    public void RemoveRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
    {
        context.Set<TEntity>().RemoveRange(entities);
    }

    // هتضيف دول جوه الكلاس (وطبعاً لازم يكون فيه using Microsoft.EntityFrameworkCore;)
    public Task<bool> AnyAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default)
    {
        return query.AnyAsync(cancellationToken);
    }

    public Task<int> CountAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default)
    {
        return query.CountAsync(cancellationToken);
    }

    public Task<List<T>> ToListAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default)
    {
        return query.ToListAsync(cancellationToken);
    }

    public Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default)
    {
        return query.FirstOrDefaultAsync(cancellationToken);
    }
    public IQueryable<T> AsNoTracking<T>(IQueryable<T> query) where T : class
    {
        return query.AsNoTracking();
    }
    // ==============================
    // 3. حفظ التغييرات
    // ==============================
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return context.SaveChangesAsync(cancellationToken);
    }
}