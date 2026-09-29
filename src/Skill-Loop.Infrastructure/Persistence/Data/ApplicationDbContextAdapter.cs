using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Entities.Booking;
using Skill_Loop.Domain.Entities.Chat;
using Skill_Loop.Domain.Entities.Courses;
using Skill_Loop.Domain.Entities.Enrollments;
using Skill_Loop.Domain.Entities.Instructors;
using Skill_Loop.Domain.Entities.Invitation;
using Skill_Loop.Domain.Entities.Notifications;
using Skill_Loop.Domain.Entities.OtpVerification;
using Skill_Loop.Domain.Entities.Session;
using Skill_Loop.Domain.Entities.Sessions;
using Skill_Loop.Domain.Entities.SiteSettings;
using Skill_Loop.Domain.Entities.Wallets;
using System.Linq.Expressions;

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
    public IQueryable<InstructorProfile> InstructorProfiles => context.InstructorProfiles;
    public IQueryable<InstructorAvailability> InstructorAvailabilities => context.InstructorAvailabilities;
    public IQueryable<InstructorReview> InstructorReviews => context.InstructorReviews;

    public IQueryable<SiteSettings> SiteSettings => context.SiteSettings;
    public IQueryable<OtpVerification> OtpVerifications => context.OtpVerifications;
    public IQueryable<StaffInvitation> StaffInvitations => context.StaffInvitations;

    // الكورسات والمحتوى
    public IQueryable<Category> Categories => context.Categories;
    public IQueryable<Course> Courses => context.Courses;
    public IQueryable<Section> Sections => context.Sections; // تمت الإضافة
    public IQueryable<Lesson> Lessons => context.Lessons;    // تمت الإضافة
    public IQueryable<CourseBookmark> CourseBookmarks => context.CourseBookmarks;
    public IQueryable<CourseReview> CourseReviews => context.CourseReviews;

    public IQueryable<Enrollment> Enrollments => context.Enrollments;
    public IQueryable<UserWallet> UserWallets => context.UserWallets;
    public IQueryable<WalletTransaction> WalletTransactions => context.WalletTransactions;

    public IQueryable<Session> Sessions => context.Sessions;
    public IQueryable<Booking> Bookings => context.Bookings;
    public IQueryable<SessionMaterial> SessionMaterials => context.SessionMaterials;

    public IQueryable<Conversation> Conversations => context.Conversations;
    public IQueryable<ChatMessage> ChatMessages => context.ChatMessages;
    public IQueryable<Notification> Notifications => context.Notifications;

    // ==============================
    // 2. عمليات الكتابة (عنصر واحد ومجموعة)
    // ==============================
    public void Add<TEntity>(TEntity entity) where TEntity : class => context.Set<TEntity>().Add(entity);
    public void Update<TEntity>(TEntity entity) where TEntity : class => context.Set<TEntity>().Update(entity);
    public void Remove<TEntity>(TEntity entity) where TEntity : class => context.Set<TEntity>().Remove(entity);

    public void AddRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class => context.Set<TEntity>().AddRange(entities);
    public void UpdateRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class => context.Set<TEntity>().UpdateRange(entities);
    public void RemoveRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class => context.Set<TEntity>().RemoveRange(entities);

    // ==============================
    // العمليات غير المتزامنة (Async Operations)
    // ==============================
    public Task<bool> AnyAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default) => query.AnyAsync(cancellationToken);
    public Task<int> CountAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default) => query.CountAsync(cancellationToken);
    public Task<List<T>> ToListAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default) => query.ToListAsync(cancellationToken);
    public Task<List<T>> ToListAsync<T>(IQueryable<T> query, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) => query.Where(predicate).ToListAsync(cancellationToken);
    public Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default) => query.FirstOrDefaultAsync(cancellationToken);
    public Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> query, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) => query.FirstOrDefaultAsync(predicate, cancellationToken);

    public IQueryable<T> AsNoTracking<T>(IQueryable<T> query) where T : class => query.AsNoTracking();

    // ==============================
    // 3. حفظ التغييرات
    // ==============================
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => context.SaveChangesAsync(cancellationToken);
}