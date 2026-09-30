using Skill_Loop.Application.Common.Errors.Category;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.External.FileStorage;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Courses;

namespace Skill_Loop.Application.Features.Categories.Commands.UpdateCategory;

public sealed class UpdateCategoryCommandHandler(
    IApplicationDbContext _dbContext,
    IFileStorage _fileStorage) : ICommandHandler<UpdateCategoryCommand>
{
    public async Task<Result> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category is null)
        {
            return Result.Failure(CategoryErrors.NotFound);
        }

        var isSlugUnique = !await _dbContext.Categories
            .AnyAsync(c => c.Slug == request.Slug.ToLower() && c.Id != request.Id, cancellationToken);

        if (!isSlugUnique)
        {
            return Result.Failure(CategoryErrors.DuplicateSlug);
        }

        string? iconUrl = category.IconUrl;
        if (request.IconStream != null && !string.IsNullOrWhiteSpace(request.IconFileName))
        {
            // حذف الصورة القديمة إذا كانت موجودة
            if (!string.IsNullOrWhiteSpace(category.IconUrl))
            {
                await _fileStorage.DeleteAsync(category.IconUrl);
            }

            var uploadResult = await _fileStorage.UploadAsync(request.IconStream, request.IconFileName, "categories");
            if (!uploadResult.IsSuccess)
            {
                return Result.Failure(uploadResult.Errors);
            }
            iconUrl = uploadResult.Data;
        }

        var updateResult = category.Update(request.Name, request.Slug, iconUrl, request.Description, request.DisplayOrder);
        if (!updateResult.IsSuccess)
        {
            return Result.Failure(updateResult.Errors);
        }

        _dbContext.Update(category);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}