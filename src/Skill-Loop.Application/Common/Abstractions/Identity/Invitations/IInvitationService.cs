using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Abstractions.Identity.Invitations;

public interface IInvitationService
{
    /// <summary>
    /// 1. إنشاء دعوة لموظف جديد
    /// </summary>
    Task<Result<string>> SendStaffInvitationAsync(
        string email,
        string role,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 2. التحقق من صحة توكن الدعوة قبل عرض صفحة التسجيل للموظف
    /// </summary>
    Task<Result<InvitationDetailsDto>> ValidateInvitationTokenAsync(
        string invitationToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 3. إبطال / إلغاء دعوة معلقة بواسطة الأدمن
    /// </summary>
    Task<Result> RevokeInvitationAsync(
        Guid invitationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 4. إكمال الموظف لبياناته وإنشاء حساب ApplicationUser والبروفايل الخاص بدوره
    /// </summary>
    Task<Result<bool>> AcceptInvitationAndCreateAccountAsync(
        string invitationToken,
        string fullName,
        string password,
        string? phoneNumber = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 5. إكمال الموظف لبياناته وإنشاء حسابه عن طريق حساب جوجل (بدون كلمة مرور)
    /// </summary>
    Task<Result<bool>> AcceptInvitationWithGoogleAsync(
        string invitationToken,
        string googleIdToken,
        CancellationToken cancellationToken = default);
}