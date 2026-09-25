using MediatR;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Events;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
// استدعاء الحدث الخاص ببيع الكورس
using Skill_Loop.Domain.Entities.Enrollments.Events;
// استدعاء كيان المحفظة
using Skill_Loop.Domain.Entities.Wallets;

namespace Skill_Loop.Application.Features.Wallets.EventHandlers.CourseEnrolled;

public sealed class CreditInstructorWalletEventHandler(
    IApplicationDbContext _dbContext) : INotificationHandler<DomainEventNotification<CourseEnrolledDomainEvent>>
{
    public async Task Handle(DomainEventNotification<CourseEnrolledDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        // 1. لو الكورس مجاني، مش هنضيف حاجة للمحفظة
        if (domainEvent.CreditsPaid <= 0) return;

        // 2. جلب الكورس لمعرفة من هو المدرب (InstructorId) واسم الكورس (عشان نكتبه في وصف الحركة)
        var course = await _dbContext.Courses
            .AsNoTracking() // AsNoTracking لأننا مش هنعدل في الكورس نفسه
            .FirstOrDefaultAsync(c => c.Id == domainEvent.CourseId, cancellationToken);

        if (course is null) return;

        // 3. جلب محفظة المدرب الفعلية
        var wallet = await _dbContext.UserWallets
            .FirstOrDefaultAsync(w => w.UserId == course.InstructorId, cancellationToken);

        // 4. (Lazy Creation) لو المدرب لسه معندوش محفظة في الداتابيز، نكريتها أوتوماتيك
        if (wallet is null)
        {
            wallet = UserWallet.Create(course.InstructorId, 0);
            _dbContext.Add(wallet);
        }

        // 5. إضافة الأرباح للمحفظة
        // نستخدم EnrollmentId كمرجع للعملية (ReferenceId) عشان نعرف الفلوس دي جت من أي عملية شراء بالظبط
        var description = $"أرباح بيع كورس: {course.Title}";
        var addResult = wallet.AddCredits(domainEvent.CreditsPaid, domainEvent.EnrollmentId, description);

        // لو حصل خطأ في الدومين (رغم إن ده نادر لأننا ضامنين إن المبلغ موجب) بنخرج
        if (addResult.IsFailure) return;

        // 6. حفظ التعديلات في قاعدة البيانات (تحديث المحفظة + حفظ حركة Transaction الجديدة)
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}