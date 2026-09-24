using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;

namespace Skill_Loop.Application.Features.Categories.Commands.CreateCategory;

public sealed record CreateCategoryCommand(
    string Name,
    string Slug,
    string? IconUrl,
    string? Description,
    int DisplayOrder) : ICommand<Guid>, ICacheInvalidatorCommand
{

    public IReadOnlyCollection<string> CacheKeys => ["categories:all"];
}