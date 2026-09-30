using Skill_Loop.Application.Common.Errors.Category;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.External.FileStorage;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Courses;

namespace Skill_Loop.Application.Features.Categories.Commands.DeleteCategory;

public sealed class DeleteCategoryCommandHandler(
    IApplicationDbContext _dbContext,
    IFileStorage _fileStorage) : ICommandHandler<DeleteCategoryCommand>
{
    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _dbContext.Categories
            .Include(c => c.Courses)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category is null)
        {
            return Result.Failure(CategoryErrors.NotFound);
        }

        if (category.Courses.Any())
        {
            return Result.Failure(CategoryErrors.HasCourses);
        }

        if (!string.IsNullOrWhiteSpace(category.IconUrl))
        {
            await _fileStorage.DeleteAsync(category.IconUrl);
        }

        _dbContext.Remove(category);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}