using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Courses;

namespace Skill_Loop.Application.Features.Categories.Commands.CreateCategory;

public sealed class CreateCategoryCommandHandler(IApplicationDbContext _dbContext) : ICommandHandler<CreateCategoryCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        // استخدام _dbContext.Categories بدلاً من Set<Category>()
        var isSlugUnique = !await _dbContext.Categories
            .AnyAsync(c => c.Slug == request.Slug.ToLower(), cancellationToken);

        if (!isSlugUnique)
        {
            return Result<Guid>.Failure(new Error("Category.DuplicateSlug", "هذا الرابط (Slug) مستخدم بالفعل لتصنيف آخر.", ErrorType.Conflict));
        }

        var categoryResult = Category.Create(request.Name, request.Slug, request.IconUrl, request.Description, request.DisplayOrder);

        if (!categoryResult.IsSuccess)
        {
            return Result<Guid>.Failure(categoryResult.Errors);
        }

        // استخدام دالة Add العادية المعرفة في الـ Interface
        _dbContext.Add(categoryResult.Data);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(categoryResult.Data.Id);
    }
}