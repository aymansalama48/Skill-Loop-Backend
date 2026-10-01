using Skill_Loop.Application.Common.Errors.Category;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.External.FileStorage;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Categories.Commands.UploadCategoryIcon;

public sealed class UploadCategoryIconCommandHandler(
    IApplicationDbContext _dbContext,
    IFileStorage _fileStorage) : ICommandHandler<UploadCategoryIconCommand, string>
{
    public async Task<Result<string>> Handle(UploadCategoryIconCommand request, CancellationToken cancellationToken)
    {
        var category = await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category is null)
        {
            return Result<string>.Failure(CategoryErrors.NotFound);
        }

        if (!string.IsNullOrWhiteSpace(category.IconUrl))
        {
            await _fileStorage.DeleteAsync(category.IconUrl);
        }

        var uploadResult = await _fileStorage.UploadAsync(request.IconStream, request.IconFileName, "categories");
        if (!uploadResult.IsSuccess)
        {
            return Result<string>.Failure(uploadResult.Errors);
        }

        category.UpdateIcon(uploadResult.Data!);
        _dbContext.Update(category);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(uploadResult.Data!);
    }
}
