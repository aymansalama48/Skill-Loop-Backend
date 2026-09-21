using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Abstractions.Identity.UserManagement;

public interface IUserManagementService
{
    Task<Result<UserDto>> GetByIdAsync(
    Guid userId,
    CancellationToken cancellationToken);

    Task<List<UserDto>> GetUsersByIdsAsync(
        IEnumerable<Guid> userIds,
        CancellationToken cancellationToken);

    Task<PagedResult<UserDto>> GetAllUsersAsync(
    int pageNumber,
    int pageSize,
    string? role,
    string? searchTerm,
    CancellationToken cancellationToken);

    Task<Result> EnsureUserExistsAsync(
    Guid userId,
    CancellationToken cancellationToken);

    Task<Result> DeactivateUserAsync(
    Guid userId,
    CancellationToken cancellationToken);

    Task<Result> ActivateUserAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<Result> UpdateProfileAsync(
    Guid userId,
    string fullName,
    string phoneNumber,
    CancellationToken cancellationToken);

    Task<Result> UpdateProfilePictureAsync(
    Guid userId,
    string avatarUrl,
    CancellationToken cancellationToken);


    Task<Result> AssignRoleAsync(
    Guid userId,
    string roleName,
    CancellationToken cancellationToken);

    Task<Result> RemoveRoleAsync(
    Guid userId,
    string roleName,
    CancellationToken cancellationToken);
}



