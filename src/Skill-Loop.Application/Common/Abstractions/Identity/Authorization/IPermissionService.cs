namespace Skill_Loop.Application.Common.Abstractions.Identity.Authorization;

/// <summary>
/// خدمة التحقق من الصلاحيات (Permissions)
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// التحقق مما إذا كان المستخدم يملك صلاحية معينة
    /// </summary>
    Task<bool> HasPermissionAsync(Guid userId, string permissionName, CancellationToken cancellationToken);

    /// <summary>
    /// الحصول على قائمة بجميع صلاحيات المستخدم
    /// </summary>
    Task<IList<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken);
}