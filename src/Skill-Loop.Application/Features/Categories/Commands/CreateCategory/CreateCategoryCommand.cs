using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Categories.Commands.CreateCategory;

[Permission(Permissions.Catalog.CategoriesManage)]
public sealed record CreateCategoryCommand(
    string Name,
    string? Description,
    int DisplayOrder) : ICommand<Guid>, ICacheInvalidatorCommand
{

    public IReadOnlyCollection<string> CacheKeys => ["categories:all"];
}