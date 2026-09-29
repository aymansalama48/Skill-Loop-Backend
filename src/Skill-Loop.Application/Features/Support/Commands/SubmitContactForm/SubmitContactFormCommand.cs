using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Support.Share;

namespace Skill_Loop.Application.Features.Support.Commands.SubmitContactForm;

/// <summary>
/// استفسار جديد من مستخدم مسجّل دخوله.
/// اسم المستخدم وبريده بيتقرؤا من الـ JWT (مش من الـ Body) عشان محدش يقدر يبعت استفسار باسم حد تاني.
/// </summary>
public sealed record SubmitContactFormCommand(
    string Subject,
    string Message,
    string Category = "General") : ICommand<Guid>, ICacheInvalidatorCommand
{
    public IReadOnlyCollection<string> CacheKeys => SupportCacheKeys.All;
}
