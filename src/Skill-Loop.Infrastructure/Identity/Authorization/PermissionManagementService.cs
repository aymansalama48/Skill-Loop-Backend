using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Core;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization.Models;
using Skill_Loop.Application.Common.Errors.Identity;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Infrastructure.Persistence.Data;
using Skill_Loop.Infrastructure.Persistence.IdentityModels;

namespace Skill_Loop.Infrastructure.Identity.Authorization;

public class PermissionManagementService(
    AppDbContext context,
    RoleManager<ApplicationRole> roleManager,
    IDateTime dateTime) : IPermissionManagementService // 👈 حقن الخدمة هنا
{
    public async Task<Result<List<PermissionDto>>> GetAllPermissionsAsync(CancellationToken cancellationToken)
    {
        var permissions = await context.Set<TbPermission>()
            .AsNoTracking()
            .OrderBy(p => p.Module)
            .Select(p => new PermissionDto(p.Id, p.Name, p.DisplayName, p.Module, p.Description))
            .ToListAsync(cancellationToken);

        return Result<List<PermissionDto>>.Success(permissions);
    }

    public async Task<Result<List<RoleWithPermissionsDto>>> GetAllRolesWithPermissionsAsync(CancellationToken cancellationToken)
    {
        var roles = await roleManager.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var result = roles.Select(r => new RoleWithPermissionsDto(
            r.Id,
            r.Name ?? string.Empty,
            r.Description,
            r.IsSystemRole,
            r.RolePermissions
                .Where(rp => rp.Permission != null)
                .Select(rp => new PermissionDto(
                    rp.Permission!.Id,
                    rp.Permission.Name,
                    rp.Permission.DisplayName,
                    rp.Permission.Module,
                    rp.Permission.Description))
                .ToList()
        )).ToList();

        return Result<List<RoleWithPermissionsDto>>.Success(result);
    }

    public async Task<Result<RoleWithPermissionsDto>> GetRolePermissionsAsync(Guid roleId, CancellationToken cancellationToken)
    {
        var role = await roleManager.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

        if (role is null)
            return Result<RoleWithPermissionsDto>.Failure(UserErrors.NotFound);

        var result = new RoleWithPermissionsDto(
            role.Id,
            role.Name ?? string.Empty,
            role.Description,
            role.IsSystemRole,
            role.RolePermissions
                .Where(rp => rp.Permission != null)
                .Select(rp => new PermissionDto(
                    rp.Permission!.Id,
                    rp.Permission.Name,
                    rp.Permission.DisplayName,
                    rp.Permission.Module,
                    rp.Permission.Description))
                .ToList()
        );

        return Result<RoleWithPermissionsDto>.Success(result);
    }

    public async Task<Result> AssignPermissionToRoleAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken)
    {
        var roleExists = await roleManager.Roles.AnyAsync(r => r.Id == roleId, cancellationToken);
        if (!roleExists) return Result.Failure(UserErrors.NotFound);

        var permissionExists = await context.Set<TbPermission>().AnyAsync(p => p.Id == permissionId, cancellationToken);
        if (!permissionExists) return Result.Failure(UserErrors.NotFound);

        var alreadyAssigned = await context.Set<TbRolePermission>()
            .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId, cancellationToken);

        if (alreadyAssigned) return Result.Success();

        var rolePermission = new TbRolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId,
            GrantedAt = dateTime.UtcNow // 👈 استخدام IDateTime هنا
        };

        await context.Set<TbRolePermission>().AddAsync(rolePermission, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken)
    {
        var rolePermission = await context.Set<TbRolePermission>()
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId, cancellationToken);

        if (rolePermission is null) return Result.Success();

        context.Set<TbRolePermission>().Remove(rolePermission);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> UpdateRolePermissionsAsync(Guid roleId, List<Guid> permissionIds, CancellationToken cancellationToken)
    {
        var role = await roleManager.FindByIdAsync(roleId.ToString());
        if (role is null) return Result.Failure(UserErrors.NotFound);

        var existingPermissions = await context.Set<TbRolePermission>()
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync(cancellationToken);

        var existingPermissionIds = existingPermissions.Select(rp => rp.PermissionId).ToList();

        var permissionsToAdd = permissionIds.Except(existingPermissionIds)
            .Select(id => new TbRolePermission
            {
                RoleId = roleId,
                PermissionId = id,
                GrantedAt = dateTime.UtcNow // 👈 واستخدام IDateTime هنا
            })
            .ToList();

        var permissionsToRemove = existingPermissions
            .Where(rp => !permissionIds.Contains(rp.PermissionId))
            .ToList();

        if (permissionsToRemove.Any())
            context.Set<TbRolePermission>().RemoveRange(permissionsToRemove);

        if (permissionsToAdd.Any())
            await context.Set<TbRolePermission>().AddRangeAsync(permissionsToAdd, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}