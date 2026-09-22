using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Domain.Entities.Invitation;
using Skill_Loop.Domain.Entities.OtpVerification;
using Skill_Loop.Domain.Entities.SiteSettings;
using Skill_Loop.Infrastructure.Persistence.IdentityModels;
using Skill_Loop.Infrastructure.Persistence.Outbox;
using System.Numerics;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;

namespace Skill_Loop.Infrastructure.Persistence.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ==============================
    // DbSets الخاصة بـ Entity Framework
    // ==============================
 



    public DbSet<SiteSettings> SiteSettings => Set<SiteSettings>();
    public DbSet<OtpVerification> OtpVerifications => Set<OtpVerification>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<StaffInvitation> StaffInvitations => Set<StaffInvitation>();

    public DbSet<Skill_Loop.Domain.Entities.Courses.Course> Courses => Set<Skill_Loop.Domain.Entities.Courses.Course>();
    public DbSet<Skill_Loop.Domain.Entities.Courses.Category> Categories => Set<Skill_Loop.Domain.Entities.Courses.Category>();
    public DbSet<Skill_Loop.Domain.Entities.Enrollments.Enrollment> Enrollments => Set<Skill_Loop.Domain.Entities.Enrollments.Enrollment>();
    public DbSet<Skill_Loop.Domain.Entities.Wallets.UserWallet> UserWallets => Set<Skill_Loop.Domain.Entities.Wallets.UserWallet>();
    public DbSet<Skill_Loop.Domain.Entities.Courses.CourseBookmark> CourseBookmarks => Set<Skill_Loop.Domain.Entities.Courses.CourseBookmark>();
    public DbSet<Skill_Loop.Domain.Entities.Courses.CourseReview> CourseReviews => Set<Skill_Loop.Domain.Entities.Courses.CourseReview>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // 1. تطبيق كل إعدادات الجداول من الـ Configurations
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // 2. تطبيق الفلتر العام للـ Soft Delete (باستخدام الدالة اللي عملناها) 👇
        builder.ApplySoftDeleteGlobalFilters();
    }
}