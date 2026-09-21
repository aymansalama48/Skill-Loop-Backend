using Skill_Loop.Application.Features.Accounts.StaffAuth.Shared;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Abstractions.Identity.Tokens;

public interface IRefreshTokenService
{
    /// <summary>
    /// إنشاء وتخزين Refresh Token جديد للموظف
    /// </summary>
    Task<string> GenerateAndSaveRefreshTokenAsync(Guid userId, CancellationToken cancellationToken);


    /// <summary>
    /// تجديد الـ Access Token باستخدام Refresh Token صالح
    /// </summary>
    Task<Result<StaffAuthResponse>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);


    /// <summary>
    /// إلغاء صلاحية Refresh Token معين
    /// </summary>
    Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);


    /// <summary>
    /// إلغاء كافة جلسات المستخدم عبر جميع الأجهزة والشركات (Global Revoke)
    /// </summary>
    Task<Result> RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken);
}