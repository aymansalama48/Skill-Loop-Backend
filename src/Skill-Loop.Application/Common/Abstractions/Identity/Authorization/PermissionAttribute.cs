namespace Skill_Loop.Application.Common.Abstractions.Identity.Authorization;

/// <summary>
/// تحديد الصلاحية المطلوبة لتنفيذ Command/Query معين
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class PermissionAttribute : Attribute
{
    public string Name { get; }

    public PermissionAttribute(string name)
    {
        Name = name;
    }
}
