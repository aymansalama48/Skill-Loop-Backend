using Microsoft.AspNetCore.Http;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Constants;
using System.Security.Claims;

namespace Skill_Loop.Infrastructure.Identity.CurrentUser;

/// <summary>
/// خدمة جلب بيانات اليوزر الحالي من الـ Claims بتاعة الـ JWT الحالي
/// </summary>
public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    // Resolved per access, not captured at construction: this service is registered Scoped and is
    // also consumed by the SaveChanges interceptors during startup seeding, where there is no
    // HttpContext yet. Caching the principal in a field would latch an anonymous user for the
    // whole scope and silently blank out the audit columns.
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    /// <summary>
    /// هل اليوزر مسجل دخوله أصلاً؟
    /// </summary>
    public bool IsAuthenticated =>
        User?.Identity?.IsAuthenticated ?? false;

    // === مشتركة بين المريض والـ Staff ===

    /// <summary>
    /// معرف الـ ApplicationUser (موجود في توكن الـ Staff)
    /// </summary>
    public Guid? UserId =>
        ParseGuid(User?.FindFirstValue(CustomClaims.UserId));

    /// <summary>
    /// الاسم الكامل
    /// </summary>
    public string? FullName =>
        User?.FindFirstValue(ClaimTypes.Name);

    // === خاصة بالمريض فقط ===

 

    /// <summary>
    /// الإيميل
    /// </summary>
    public string? Email =>
        User?.FindFirstValue(ClaimTypes.Email);

    /// <summary>
    /// أول دور لليوزر
    /// </summary>
    public string? Role =>
        User?.FindFirstValue(ClaimTypes.Role);

    /// <summary>
    /// كل الأدوار
    /// </summary>
    public IReadOnlyList<string> Roles =>
        User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
        ?? new List<string>();

    /// <summary>
    /// هل اليوزر في دور معين؟
    /// </summary>
    public bool IsInRole(string role) =>
        User?.IsInRole(role) ?? false;

    /// <summary>
    /// هل اليوزر معاه صلاحية معينة؟
    /// </summary>
    public bool HasPermission(string permission) =>
        User?.FindAll(CustomClaims.Permission).Any(c => c.Value == permission) ?? false;

    /// <summary>
    /// كل صلاحيات اليوزر
    /// </summary>
    public IEnumerable<string> GetPermissions() =>
        User?.FindAll(CustomClaims.Permission).Select(c => c.Value)
        ?? Enumerable.Empty<string>();

    private static Guid? ParseGuid(string? value) =>
        Guid.TryParse(value, out var guid) ? guid : null;
}