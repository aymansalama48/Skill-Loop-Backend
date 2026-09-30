namespace Skill_Loop.Application.Common.Abstractions.Identity.Authorization;

/// <summary>
/// يحدد أن الـ Command/Query يتطلب فقط أن يكون المستخدم مسجلاً للدخول، ولا يتطلب صلاحيات معينة.
/// يُستخدم غالباً للعمليات التي ينفذها المستخدم على بياناته الخاصة (مثل عرض محفظته، حجوزاته، إلخ).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class AuthenticatedOnlyAttribute : Attribute
{
}
