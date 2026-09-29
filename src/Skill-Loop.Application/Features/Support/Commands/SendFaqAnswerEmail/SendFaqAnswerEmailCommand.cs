using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Support.Share;

namespace Skill_Loop.Application.Features.Support.Commands.SendFaqAnswerEmail;

/// <summary>
/// المستخدم لقى إجابته في الـ FAQ وطلب إرسالها ليه على الإيميل.
/// بيشتغل على سؤال منشور بس (عشان مفيش حد يطلب إجابة سؤال لسه متجاوبش عليه).
/// </summary>
public sealed record SendFaqAnswerEmailCommand(Guid Id) : ICommand, ICacheInvalidatorCommand
{
    public IReadOnlyCollection<string> CacheKeys => [SupportCacheKeys.QuestionById + Id];
}
