
namespace Skill_Loop.UnitTests.Common;

using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Entities.Booking;
using Skill_Loop.Domain.Entities.Courses;
using Skill_Loop.Domain.Entities.Enrollments;
using Skill_Loop.Domain.Entities.Instructors;
using Skill_Loop.Domain.Entities.Invitation;
using Skill_Loop.Domain.Entities.OtpVerification;
using Skill_Loop.Domain.Entities.Session;
using Skill_Loop.Domain.Entities.Sessions;
using Skill_Loop.Domain.Entities.SiteSettings;
using Skill_Loop.Domain.Entities.Wallets;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

public static class InMemoryDbContextHelper
{
    public static IApplicationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<Skill_Loop.Infrastructure.Persistence.Data.AppDbContext>()
            .UseInMemoryDatabase("TestDb_" + System.Guid.NewGuid())
            .Options;

        var dbContext = new InMemoryAppDbContext(options);
        return new InMemoryDbContextWrapper(dbContext);
    }

    private class InMemoryDbContextWrapper : IApplicationDbContext
    {
        private readonly Skill_Loop.Infrastructure.Persistence.Data.AppDbContext _dbContext;

        public InMemoryDbContextWrapper(Skill_Loop.Infrastructure.Persistence.Data.AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IQueryable<SiteSettings> SiteSettings => _dbContext.SiteSettings;
        public IQueryable<OtpVerification> OtpVerifications => _dbContext.OtpVerifications;
        public IQueryable<StaffInvitation> StaffInvitations => _dbContext.StaffInvitations;
        public IQueryable<Course> Courses => _dbContext.Courses;
        public IQueryable<Category> Categories => _dbContext.Categories;
        public IQueryable<Enrollment> Enrollments => _dbContext.Enrollments;
        public IQueryable<UserWallet> UserWallets => _dbContext.UserWallets;
        public IQueryable<CourseBookmark> CourseBookmarks => _dbContext.CourseBookmarks;
        public IQueryable<CourseReview> CourseReviews => _dbContext.CourseReviews;
        public IQueryable<Session> Sessions => _dbContext.Sessions;
        public IQueryable<Booking> Bookings => _dbContext.Bookings;
        public IQueryable<SessionMaterial> SessionMaterials => _dbContext.SessionMaterials;
        public IQueryable<Skill_Loop.Domain.Entities.Chat.Conversation> Conversations => _dbContext.Conversations;
        public IQueryable<Skill_Loop.Domain.Entities.Chat.ChatMessage> ChatMessages => _dbContext.ChatMessages;
        public IQueryable<Skill_Loop.Domain.Entities.Notifications.Notification> Notifications => _dbContext.Notifications;

        public IQueryable<InstructorProfile> InstructorProfiles => _dbContext.InstructorProfiles;

        public IQueryable<InstructorReview> InstructorReviews => _dbContext.InstructorReviews;

        public IQueryable<WalletTransaction> WalletTransactions => _dbContext.WalletTransactions;

        public IQueryable<InstructorAvailability> InstructorAvailabilities => _dbContext.InstructorAvailabilities;

        public IQueryable<Section> Sections => throw new NotImplementedException();

        public IQueryable<Lesson> Lessons => throw new NotImplementedException();

        public void Add<TEntity>(TEntity entity) where TEntity : class => _dbContext.Add(entity);
        public void Update<TEntity>(TEntity entity) where TEntity : class => _dbContext.Update(entity);
        public void Remove<TEntity>(TEntity entity) where TEntity : class => _dbContext.Remove(entity);
        public void AddRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class => _dbContext.AddRange(entities);
        public void UpdateRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class => _dbContext.UpdateRange(entities);
        public void RemoveRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class => _dbContext.RemoveRange(entities);

        public Task<bool> AnyAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default) => query.AnyAsync(cancellationToken);
        public Task<int> CountAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default) => query.CountAsync(cancellationToken);
        public Task<List<T>> ToListAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default) => query.ToListAsync(cancellationToken);
        public Task<List<T>> ToListAsync<T>(IQueryable<T> query, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) => query.Where(predicate).ToListAsync(cancellationToken);
        public Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default) => query.FirstOrDefaultAsync(cancellationToken);
        public Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> query, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) => query.FirstOrDefaultAsync(predicate, cancellationToken);
        public IQueryable<T> AsNoTracking<T>(IQueryable<T> query) where T : class => query.AsNoTracking();
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => _dbContext.SaveChangesAsync(cancellationToken);
    }
}