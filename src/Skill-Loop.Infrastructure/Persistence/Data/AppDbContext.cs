using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Domain.Common.Entities;
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
using Skill_Loop.Domain.Entities.Support;
using Skill_Loop.Domain.Entities.Wallets;
using Skill_Loop.Infrastructure.Persistence.IdentityModels;
using Skill_Loop.Infrastructure.Persistence.Outbox;
using System.Reflection;

namespace Skill_Loop.Infrastructure.Persistence.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ==============================
    // DbSets الخاصة بـ Entity Framework
    // ==============================

    public DbSet<InstructorProfile> InstructorProfiles => Set<InstructorProfile>();
    public DbSet<InstructorAvailability> InstructorAvailabilities => Set<InstructorAvailability>();
    public DbSet<InstructorReview> InstructorReviews => Set<InstructorReview>();

    public DbSet<SiteSettings> SiteSettings => Set<SiteSettings>();
    public DbSet<OtpVerification> OtpVerifications => Set<OtpVerification>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<StaffInvitation> StaffInvitations => Set<StaffInvitation>();

    // الكورسات والمحتوى
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Section> Sections => Set<Section>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<CourseBookmark> CourseBookmarks => Set<CourseBookmark>();
    public DbSet<CourseReview> CourseReviews => Set<CourseReview>();

    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<UserWallet> UserWallets => Set<UserWallet>();
    public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();

    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<SessionMaterial> SessionMaterials => Set<SessionMaterial>();
    public DbSet<SessionReview> SessionReviews => Set<SessionReview>();

    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<Notification> Notifications => Set<Notification>();

    // 📞 الدعم الفني
    public DbSet<SupportQuestion> SupportQuestions => Set<SupportQuestion>();

    // 🏷️ الـ Promo Codes والـ Credit Purchases (محددة بالـ Fully Qualified Namespace لمنع أي تضارب)
    public DbSet<Skill_Loop.Domain.Entities.Promotions.PromoCode> PromoCodes => Set<Skill_Loop.Domain.Entities.Promotions.PromoCode>();
    public DbSet<Skill_Loop.Domain.Entities.Promotions.PromoRedemption> PromoRedemptions => Set<Skill_Loop.Domain.Entities.Promotions.PromoRedemption>();
    public DbSet<Skill_Loop.Domain.Entities.Wallets.CreditPurchase> CreditPurchases => Set<Skill_Loop.Domain.Entities.Wallets.CreditPurchase>();

    // 📧 Email Logs
    public DbSet<Skill_Loop.Domain.Entities.Emails.EmailLog> EmailLogs => Set<Skill_Loop.Domain.Entities.Emails.EmailLog>();

    // 📜 Audit Logs
    public DbSet<Skill_Loop.Domain.Entities.System.AuditLog> AuditLogs => Set<Skill_Loop.Domain.Entities.System.AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        foreach (var entityType in builder.Model.GetEntityTypes()
            .Where(e => typeof(BaseEntity).IsAssignableFrom(e.ClrType)))
        {
            builder.Entity(entityType.ClrType)
                .Property(nameof(BaseEntity.Id))
                .ValueGeneratedNever();
        }

        builder.ApplySoftDeleteGlobalFilters();
    }
}