using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Abstractions.Notifications;

/// <summary>
/// إشعارات الإجابات — بتستهلكها عمليات الرد على الاستفسار وإرسال إجابة الـ FAQ.
/// </summary>
public interface ISupportAnswerNotifier
{
    /// <summary>
    /// إيميل للمستخدم فيه رد فريق الدعم على استفساره.
    /// </summary>
    Task<Result> SendAnswerNotificationAsync(
        string toEmail,
        string userName,
        string question,
        string answer,
        string category,
        string questionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// إيميل للمستخدم فيه إجابة جاهزة من الـ FAQ، لما يلاقي إجابته موجودة ويسأل إنها تتبعتله.
    /// </summary>
    Task<Result> SendFaqAnswerAsync(
        string toEmail,
        string userName,
        string question,
        string answer,
        string questionId,
        CancellationToken cancellationToken = default);
}
