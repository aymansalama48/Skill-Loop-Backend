using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;

namespace Skill_Loop.Application.Features.Categories.Commands.UpdateCategory;

public sealed record UpdateCategoryCommand(
    Guid Id,
    string Name,
    string Slug,
Stream? IconStream,
    string? IconFileName,
    string? Description,
    int DisplayOrder) : ICommand, ICacheInvalidatorCommand
{
    public IReadOnlyCollection<string> CacheKeys => ["categories:all"];
}