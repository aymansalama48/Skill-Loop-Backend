using Skill_Loop.Application.Common.Errors.Course;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Courses.Commands.AddSection;

public sealed class AddSectionCommandHandler : ICommandHandler<AddSectionCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public AddSectionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(AddSectionCommand request, CancellationToken cancellationToken)
    {
        // 1. السحر هنا: بنستخدم AsNoTracking عشان نمنع EF Core من تتبع الكورس وتحديثه بالغلط
        // وشيلنا Include لأننا مش محتاجين نحمل الأقسام القديمة عشان نضيف واحد جديد
        var course = await _context.FirstOrDefaultAsync(
            _context.AsNoTracking(_context.Courses).Where(c => c.Id == request.CourseId),
            cancellationToken);

        if (course is null)
        {
            return Result<Guid>.Failure(CourseErrors.NotFound);
        }

        // 2. تطبيق البيزنس لوجيك (القسم هينضاف في الذاكرة)
        var addSectionResult = course.AddSection(
            request.Title,
            request.OrderIndex);

        if (addSectionResult.IsFailure)
        {
            return Result<Guid>.Failure(addSectionResult.Errors.First());
        }

        // 3. نستخرج القسم الجديد (بما إننا معملناش Include للقديم، فاللستة مفهاش غير الجديد بس)
        var newSection = course.Sections.First();

        // 4. ندي أمر صريح لـ EF بإضافة القسم (INSERT) فقط وتجاهل تحديث الكورس (UPDATE)
        _context.Add(newSection);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(newSection.Id);
    }
}