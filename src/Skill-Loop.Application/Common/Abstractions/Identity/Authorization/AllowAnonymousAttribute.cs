namespace Skill_Loop.Application.Common.Abstractions.Identity.Authorization;

/// <summary>
/// يحدد أن الـ Command/Query متاح للجميع ولا يحتاج لتسجيل دخول.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class AllowAnonymousAttribute : Attribute
{
}
