using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Categories.Commands.UploadCategoryIcon;

[Permission(Permissions.Catalog.CategoriesManage)]
public sealed record UploadCategoryIconCommand(
    Guid Id,
    Stream IconStream,
    string IconFileName) : ICommand<string>, ICacheInvalidatorCommand
{
    public IReadOnlyCollection<string> CacheKeys => ["categories:all"];
}
