using Skill_Loop.Application.Common.Errors.Category;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.External.FileStorage;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Courses;

namespace Skill_Loop.Application.Features.Categories.Commands.CreateCategory;

public sealed class CreateCategoryCommandHandler(
    IApplicationDbContext _dbContext,
    IFileStorage _fileStorage) : ICommandHandler<CreateCategoryCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var isSlugUnique = !await _dbContext.Categories
            .AnyAsync(c => c.Slug == request.Slug.ToLower(), cancellationToken);

        if (!isSlugUnique)
        {
            return Result<Guid>.Failure(CategoryErrors.DuplicateSlug);
        }

        string? iconUrl = null;
        if (request.IconStream != null && !string.IsNullOrWhiteSpace(request.IconFileName))
        {
            var uploadResult = await _fileStorage.UploadAsync(request.IconStream, request.IconFileName, "categories");
            if (!uploadResult.IsSuccess)
            {
                return Result<Guid>.Failure(uploadResult.Errors);
            }
            iconUrl = uploadResult.Data;
        }

        var categoryResult = Category.Create(request.Name, request.Slug, iconUrl, request.Description, request.DisplayOrder);
        if (!categoryResult.IsSuccess)
        {
            return Result<Guid>.Failure(categoryResult.Errors);
        }

        _dbContext.Add(categoryResult.Data);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(categoryResult.Data.Id);
    }
}