using Microsoft.EntityFrameworkCore;
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

namespace Skill_Loop.Application.Common.Abstractions.Persistence.Data;

public interface IApplicationDbContext
{
    // ==============================
    // 1. عمليات القراءة (IQueryable)
    // ==============================
    public IQueryable<InstructorProfile> InstructorProfiles { get; }
    public IQueryable<InstructorAvailability> InstructorAvailabilities { get; }
    public IQueryable<InstructorReview> InstructorReviews { get; }

    public IQueryable<SiteSettings> SiteSettings { get; }
    public IQueryable<OtpVerification> OtpVerifications { get; }
    public IQueryable<StaffInvitation> StaffInvitations { get; }

    // الكورسات والمحتوى (تمت إضافة الأقسام والدروس هنا)
    public IQueryable<Category> Categories { get; }
    public IQueryable<Course> Courses { get; }
    public IQueryable<Section> Sections { get; }
    public IQueryable<Lesson> Lessons { get; }
    public IQueryable<CourseBookmark> CourseBookmarks { get; }
    public IQueryable<CourseReview> CourseReviews { get; }

    public IQueryable<Enrollment> Enrollments { get; }
    public IQueryable<UserWallet> UserWallets { get; }
    public IQueryable<WalletTransaction> WalletTransactions { get; }

    public IQueryable<Session> Sessions { get; }
    public IQueryable<Booking> Bookings { get; }
    public IQueryable<SessionMaterial> SessionMaterials { get; }

    public IQueryable<Conversation> Conversations { get; }
    public IQueryable<ChatMessage> ChatMessages { get; }
    public IQueryable<Notification> Notifications { get; }

    // ==============================
    // 2. عمليات الكتابة والإضافة والحذف
    // ==============================
    void Add<TEntity>(TEntity entity) where TEntity : class;
    void Update<TEntity>(TEntity entity) where TEntity : class;
    void Remove<TEntity>(TEntity entity) where TEntity : class;

    void AddRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;
    void UpdateRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;
    void RemoveRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;

    Task<bool> AnyAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default);
    Task<int> CountAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default);
    Task<List<T>> ToListAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default);
    Task<List<T>> ToListAsync<T>(IQueryable<T> query, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default);
    Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> query, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    IQueryable<T> AsNoTracking<T>(IQueryable<T> query) where T : class;

    // ==============================
    // 3. حفظ التغييرات
    // ==============================
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}