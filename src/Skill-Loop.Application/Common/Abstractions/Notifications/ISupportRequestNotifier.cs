using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Abstractions.Notifications;

/// <summary>
/// إشعارات استلام استفسار جديد — بتستهلكها عملية إنشاء الاستفسار بس.
/// </summary>
public interface ISupportRequestNotifier
{
    /// <summary>
    /// إيميل للمستخدم أثناء استلام الاستفسار: تأكيد إن العميل وصله وسيرد عليه الفريق قريباً.
    /// </summary>
    Task<Result> SendContactFormConfirmationAsync(
        string toEmail,
        string userName,
        string subject,
        string questionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// إيميل لفريق الدعم عند وصول استفسار جديد من عميل.
    /// </summary>
    Task<Result> SendSupportTeamNotificationAsync(
        string userName,
        string userEmail,
        string subject,
        string message,
        string category,
        string questionId,
        CancellationToken cancellationToken = default);
}
