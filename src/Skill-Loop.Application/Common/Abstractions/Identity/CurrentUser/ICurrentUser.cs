namespace Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    Guid? UserId { get; }
    string? FullName { get; }


    string? Email { get; }

    string? Role { get; }

    IReadOnlyList<string> Roles { get; }

    bool IsInRole(string role);

    bool HasPermission(string permission);
    IEnumerable<string> GetPermissions();
}