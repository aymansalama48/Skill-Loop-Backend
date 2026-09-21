namespace Skill_Loop.Application.Common.Abstractions.Identity.Authorization.Models;

// DTO لتمثيل الصلاحية الواحدة
public record PermissionDto(
    Guid Id,
    string Name,
    string DisplayName,
    string Module,
    string? Description);