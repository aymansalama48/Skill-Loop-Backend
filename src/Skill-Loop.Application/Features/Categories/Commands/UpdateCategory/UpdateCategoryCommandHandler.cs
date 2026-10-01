using Skill_Loop.Application.Common.Errors.Category;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.External.FileStorage;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Courses;

namespace Skill_Loop.Application.Features.Categories.Commands.UpdateCategory;

public sealed class UpdateCategoryCommandHandler(
    IApplicationDbContext _dbContext) : ICommandHandler<UpdateCategoryCommand>
{
    public async Task<Result> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category is null)
        {
            return Result.Failure(CategoryErrors.NotFound);
        }

        var updateResult = category.Update(request.Name, category.IconUrl, request.Description, request.DisplayOrder);
        if (!updateResult.IsSuccess)
        {
            return Result.Failure(updateResult.Errors);
        }

        _dbContext.Update(category);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}