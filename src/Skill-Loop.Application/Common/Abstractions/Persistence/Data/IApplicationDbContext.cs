using Microsoft.EntityFrameworkCore;
using Skill_Loop.Domain.Entities.Booking;
using Skill_Loop.Domain.Entities.Instructors;
using Skill_Loop.Domain.Entities.Invitation;
using Skill_Loop.Domain.Entities.OtpVerification;
using Skill_Loop.Domain.Entities.Session;
using Skill_Loop.Domain.Entities.Sessions;
using Skill_Loop.Domain.Entities.SiteSettings;
using System.Linq.Expressions;

namespace Skill_Loop.Application.Common.Abstractions.Persistence.Data;

public interface IApplicationDbContext
{
    // 1. عمليات القراءة
    // 1. عمليات القراءة (استخدمنا IQueryable المستقلة بدلاً من DbSet)

    //IQueryable<> ...... { get; } <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

    public IQueryable<InstructorProfile> InstructorProfiles { get; }
    public IQueryable<InstructorAvailability> InstructorAvailabilities { get; } 
    public IQueryable<InstructorReview> InstructorReviews { get; }

    public IQueryable<SiteSettings> SiteSettings { get; }
    public IQueryable<OtpVerification> OtpVerifications { get; }
    public IQueryable<StaffInvitation> StaffInvitations { get; }
    public IQueryable<Skill_Loop.Domain.Entities.Courses.Course> Courses { get; }
    public IQueryable<Skill_Loop.Domain.Entities.Courses.Category> Categories { get; }
    public IQueryable<Skill_Loop.Domain.Entities.Enrollments.Enrollment> Enrollments { get; }
    public IQueryable<Skill_Loop.Domain.Entities.Wallets.UserWallet> UserWallets { get; }
    public IQueryable<Skill_Loop.Domain.Entities.Wallets.WalletTransaction> WalletTransactions { get; }
    public IQueryable<Skill_Loop.Domain.Entities.Courses.CourseBookmark> CourseBookmarks { get; }
    public IQueryable<Skill_Loop.Domain.Entities.Courses.CourseReview> CourseReviews { get; }

    public IQueryable<Session> Sessions { get; }
    public IQueryable<Booking> Bookings { get; }
    public IQueryable<SessionMaterial> SessionMaterials { get; }

    // 💬 الشات
    public IQueryable<Skill_Loop.Domain.Entities.Chat.Conversation> Conversations { get; }
    public IQueryable<Skill_Loop.Domain.Entities.Chat.ChatMessage> ChatMessages { get; }

    // 🔔 إشعارات جوه التطبيق (In-App)
    public IQueryable<Skill_Loop.Domain.Entities.Notifications.Notification> Notifications { get; }


    // 2. عمليات الكتابة والإضافة والحذف (Generic Methods)
    void Add<TEntity>(TEntity entity) where TEntity : class;
    void Update<TEntity>(TEntity entity) where TEntity : class;
    void Remove<TEntity>(TEntity entity) where TEntity : class;

    // عمليات المجموعة (Range Operations)
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

    // 3. حفظ التغييرات
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}