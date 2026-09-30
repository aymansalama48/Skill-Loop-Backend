using Skill_Loop.Application.Common.Errors.Category;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Abstractions.External.FileStorage; // مسار الانترفيس بتاعك
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Courses;

namespace Skill_Loop.Application.Features.Courses.Commands.CreateCourse;

public sealed class CreateCourseCommandHandler : ICommandHandler<CreateCourseCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorage _fileStorage; // حقن خدمة الملفات

    public CreateCourseCommandHandler(
        IApplicationDbContext context,
        IFileStorage fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }

    public async Task<Result<Guid>> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
    {
        // 1. التأكد من وجود القسم
        var categoryExists = await _context.AnyAsync(
            _context.Categories.Where(c => c.Id == request.CategoryId),
            cancellationToken);

        if (!categoryExists)
        {
            return Result<Guid>.Failure(CategoryErrors.NotFound);
        }

        // 2. معالجة ورفع الصورة لو اليوزر بعتها
        string uploadedThumbnailUrl = string.Empty;

        if (request.ThumbnailStream is not null && !string.IsNullOrWhiteSpace(request.ThumbnailFileName))
        {
            // استخدام دالة الرفع وتمرير اسم الفولدر "Courses" كـ Parameter تالت
            var uploadResult = await _fileStorage.UploadAsync(
                request.ThumbnailStream,
                request.ThumbnailFileName,
                "Courses");

            // لو الرفع فشل، بنرجع الإيرور فوراً
            if (uploadResult.IsFailure)
            {
                return Result<Guid>.Failure(uploadResult.Errors.First());
            }

            // لو نجح، بناخد مسار الصورة
            uploadedThumbnailUrl = uploadResult.Data!;
        }

        // 3. إنشاء الكورس بالرابط (أو نص فارغ لو مفيش صورة)
        var courseResult = Course.Create(
            request.Title,
            request.Description,
            uploadedThumbnailUrl,
            request.Credits,
            request.Level,
            request.InstructorId,
            request.InstructorName,
            request.CategoryId);

        if (courseResult.IsFailure)
        {
            return Result<Guid>.Failure(courseResult.Errors.First());
        }

        var course = courseResult.Data!;
        _context.Add(course);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(course.Id);
    }
}