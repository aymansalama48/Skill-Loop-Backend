using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Categories.Commands.DeleteCategory;

[AuthenticatedOnly]
public sealed record DeleteCategoryCommand(Guid Id) : ICommand, ICacheInvalidatorCommand
{
    public IReadOnlyCollection<string> CacheKeys => ["categories:all"];
}