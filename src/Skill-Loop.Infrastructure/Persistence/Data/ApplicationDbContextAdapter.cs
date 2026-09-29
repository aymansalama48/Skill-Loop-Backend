
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Entities.Booking;
using Skill_Loop.Domain.Entities.Courses;
using Skill_Loop.Domain.Entities.Enrollments;
using Skill_Loop.Domain.Entities.Invitation;
using Skill_Loop.Domain.Entities.OtpVerification;
using Skill_Loop.Domain.Entities.Promotions;
using Skill_Loop.Domain.Entities.Session;
using Skill_Loop.Domain.Entities.SessionMaterial;
using Skill_Loop.Domain.Entities.SiteSettings;
using Skill_Loop.Domain.Entities.Wallets;

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

    public IQueryable<SiteSettings> SiteSettings => context.Set<SiteSettings>();
    public IQueryable<OtpVerification> OtpVerifications => context.Set<OtpVerification>();
    public IQueryable<StaffInvitation> StaffInvitations => context.Set<StaffInvitation>();
    public IQueryable<Course> Courses => context.Set<Course>();
    public IQueryable<Category> Categories => context.Set<Category>();
    public IQueryable<Enrollment> Enrollments => context.Set<Enrollment>();
    public IQueryable<UserWallet> UserWallets => context.Set<UserWallet>();
    public IQueryable<CourseBookmark> CourseBookmarks => context.Set<CourseBookmark>();
    public IQueryable<CourseReview> CourseReviews => context.Set<CourseReview>();

    public IQueryable<Session> Sessions => context.Set<Session>();
    public IQueryable<Booking> Bookings => context.Set<Booking>();
    public IQueryable<SessionMaterial> SessionMaterials => context.Set<SessionMaterial>();

    // 💬 الشات
    public IQueryable<Skill_Loop.Domain.Entities.Chat.Conversation> Conversations => context.Set<Skill_Loop.Domain.Entities.Chat.Conversation>();
    public IQueryable<Skill_Loop.Domain.Entities.Chat.ChatMessage> ChatMessages => context.Set<Skill_Loop.Domain.Entities.Chat.ChatMessage>();

    // 🔔 إشعارات جوه التطبيق (In-App)
    public IQueryable<Skill_Loop.Domain.Entities.Notifications.Notification> Notifications => context.Set<Skill_Loop.Domain.Entities.Notifications.Notification>();

    // 🏷️ الـ Promo Codes والـ Credit Purchases
    public IQueryable<PromoCode> PromoCodes => context.Set<PromoCode>();
    public IQueryable<PromoRedemption> PromoRedemptions => context.Set<PromoRedemption>();
    public IQueryable<CreditPurchase> CreditPurchases => context.Set<CreditPurchase>();

    // ==============================
    // 2. عمليات الكتابة (عنصر واحد)
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
    // عمليات الكتابة (مجموعة Range)
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