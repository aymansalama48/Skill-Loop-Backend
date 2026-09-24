using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Courses;

namespace Skill_Loop.Application.Features.Categories.Commands.UpdateCategory;

public sealed class UpdateCategoryCommandHandler(IApplicationDbContext _dbContext) : ICommandHandler<UpdateCategoryCommand>
{
    public async Task<Result> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category is null)
        {
            return Result.Failure(new Error("Category.NotFound", "التصنيف غير موجود.", ErrorType.NotFound));
        }

        var isSlugUnique = !await _dbContext.Categories
            .AnyAsync(c => c.Slug == request.Slug.ToLower() && c.Id != request.Id, cancellationToken);

        if (!isSlugUnique)
        {
            return Result.Failure(new Error("Category.DuplicateSlug", "هذا الرابط (Slug) مستخدم بالفعل لتصنيف آخر.", ErrorType.Conflict));
        }

        var updateResult = category.Update(request.Name, request.Slug, request.IconUrl, request.Description, request.DisplayOrder);

        if (!updateResult.IsSuccess)
        {
            return Result.Failure(updateResult.Errors);
        }

        // استخدام دالة Update المعرفة في الـ Interface
        _dbContext.Update(category);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}