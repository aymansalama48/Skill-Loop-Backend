using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Skill_Loop.Application.Common.Abstractions.Core;
using Skill_Loop.Application.Common.Abstractions.Identity.Tokens;
using Skill_Loop.Application.Common.Abstractions.Identity.UserManagement;
using Skill_Loop.Application.Common.Errors.Identity;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Infrastructure.Persistence.IdentityModels;
using Microsoft.EntityFrameworkCore;

namespace Skill_Loop.Infrastructure.Identity.UserManagement;

/// <summary>
/// تنفيذ خدمة إدارة المستخدمين (تخدم Staff والمريض صاحب الحساب الدائم)
/// </summary>
public class UserManagementService(
    UserManager<ApplicationUser> userManager,
    IRefreshTokenService refreshTokenService,
    IDateTime dateTime,
    ILogger<UserManagementService> logger) : IUserManagementService
{
    /// <summary>
    /// جلب بيانات المستخدم بواسطة المعرف
    /// </summary>
    public async Task<Result<UserDto>> GetByIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Result<UserDto>.Failure(UserErrors.NotFound);

        var roles = await userManager.GetRolesAsync(user);

        return Result<UserDto>.Success(new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            Email = user.Email!,
            PhoneNumber = user.PhoneNumber,
            AvatarUrl = user.AvatarUrl,
            Roles = roles.ToList(),
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        });
    }
    public async Task<Result<UserDto>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return Result<UserDto>.Failure(UserErrors.NotFound);

        return Result<UserDto>.Success(new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email!,
            EmailConfirmed = user.EmailConfirmed 
        });
    }
    public async Task<List<UserDto>> GetUsersByIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken)
    {
        return await userManager.Users
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new UserDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                FullName = u.FullName,
                AvatarUrl = u.AvatarUrl,
                Email = u.Email!,
                IsActive = u.IsActive
            })
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// التأكد من وجود المستخدم في النظام
    /// </summary>
    public async Task<Result> EnsureUserExistsAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Result.Failure(UserErrors.NotFound);

        return Result.Success();
    }

    /// <summary>
    /// تعطيل تسجيل الدخول + إلغاء كل الجلسات الشغالة
    /// </summary>
    public async Task<Result> DeactivateUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Result.Failure(UserErrors.NotFound);

        if (!user.IsActive)
            return Result.Success("الحساب معطل بالفعل");

        user.IsActive = false;
        var result = await userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogWarning("فشل تعطيل المستخدم {UserId}: {Errors}", userId, errors);
            return Result.Failure(UserErrors.UpdateFailed(errors));
        }

        await refreshTokenService.RevokeAllUserTokensAsync(userId, cancellationToken);

        logger.LogInformation("تم تعطيل المستخدم {UserId} وإلغاء كل جلساته بنجاح", userId);
        return Result.Success("تم تعطيل الحساب وإنهاء كل الجلسات بنجاح");
    }

    /// <summary>
    /// إعادة تفعيل الحساب
    /// </summary>
    public async Task<Result> ActivateUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Result.Failure(UserErrors.NotFound);

        if (user.IsActive)
            return Result.Success("الحساب مفعل بالفعل");

        user.IsActive = true;
        var result = await userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogWarning("فشل تفعيل المستخدم {UserId}: {Errors}", userId, errors);
            return Result.Failure(UserErrors.UpdateFailed(errors));
        }

        logger.LogInformation("تم تفعيل المستخدم {UserId} بنجاح", userId);
        return Result.Success("تم تفعيل الحساب بنجاح");
    }

    /// <summary>
    /// تحديث البيانات الأساسية
    /// </summary>
    public async Task<Result> UpdateProfileAsync(
        Guid userId,
        string fullName,
        string phoneNumber,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Result.Failure(UserErrors.NotFound);

        if (string.IsNullOrWhiteSpace(fullName))
            return Result.Failure(UserErrors.ValidationFailed("الاسم مطلوب"));

        var nameParts = fullName.Trim().Split(' ', 2);
        user.FirstName = nameParts[0];
        user.LastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;

        if (!string.IsNullOrWhiteSpace(phoneNumber))
        {
            var existingUser = await userManager.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber && u.Id != userId, cancellationToken);

            if (existingUser is not null)
                return Result.Failure(UserErrors.PhoneAlreadyExists);

            user.PhoneNumber = phoneNumber;
        }

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogWarning("فشل تحديث الملف الشخصي للمستخدم {UserId}: {Errors}", userId, errors);
            return Result.Failure(UserErrors.UpdateFailed(errors));
        }

        logger.LogInformation("تم تحديث الملف الشخصي للمستخدم {UserId} بنجاح", userId);
        return Result.Success("تم تحديث البيانات بنجاح");
    }

    /// <summary>
    /// تحديث رابط الصورة الشخصية
    /// </summary>
    public async Task<Result> UpdateProfilePictureAsync(
        Guid userId,
        string avatarUrl,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Result.Failure(UserErrors.NotFound);

        if (!string.IsNullOrWhiteSpace(avatarUrl) && !Uri.IsWellFormedUriString(avatarUrl, UriKind.Absolute))
            return Result.Failure(UserErrors.ValidationFailed("رابط الصورة غير صالح"));

        user.AvatarUrl = avatarUrl;
        var result = await userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogWarning("فشل تحديث صورة المستخدم {UserId}: {Errors}", userId, errors);
            return Result.Failure(UserErrors.UpdateFailed(errors));
        }

        logger.LogInformation("تم تحديث صورة المستخدم {UserId} بنجاح", userId);
        return Result.Success("تم تحديث الصورة بنجاح");
    }

    /// <summary>
    /// جلب قائمة المستخدمين لمدير النظام (CRM) مع البحث والتصفية وتقسيم الصفحات
    /// </summary>
    public async Task<PagedResult<UserDto>> GetAllUsersAsync(
        int pageNumber,
        int pageSize,
        string? role,
        string? searchTerm,
        CancellationToken cancellationToken)
    {
        var query = userManager.Users.AsQueryable();

        // 1. فلترة بالدور (Role)
        if (!string.IsNullOrWhiteSpace(role))
        {
            var usersInRole = await userManager.GetUsersInRoleAsync(role);
            var userIdsInRole = usersInRole.Select(u => u.Id).ToList();
            query = query.Where(u => userIdsInRole.Contains(u.Id));
        }

        // 2. فلترة بنص البحث (SearchTerm)
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(u =>
                (u.FirstName != null && u.FirstName.ToLower().Contains(term)) ||
                (u.LastName != null && u.LastName.ToLower().Contains(term)) ||
                (u.Email != null && u.Email.ToLower().Contains(term)) ||
                (u.PhoneNumber != null && u.PhoneNumber.Contains(term))
            );
        }

        // 3. حساب إجمالي عدد العناصر
        var totalCount = await query.CountAsync(cancellationToken);

        // 4. جلب عناصر الصفحة الحالية
        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // 5. تحويل البيانات (Mapping) وإضافة الأدوار
        var userDtos = new List<UserDto>();
        foreach (var user in users)
        {
            var userRoles = await userManager.GetRolesAsync(user);

            userDtos.Add(new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber,
                AvatarUrl = user.AvatarUrl,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                Roles = userRoles.ToList()
            });
        }

        // 6. تجهيز الرد في شكل PagedResult مباشر
        var pagedResult = new PagedResult<UserDto>
        {
            Items = userDtos,
            Pagination = new PaginationMetadata
            {
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            }
        };

        return pagedResult; // 👈 التعديل هنا: إرجاع مباشر بدون Result.Success
    }

    public async Task<Result> AssignRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Result.Failure(UserErrors.NotFound);

        // لو اليوزر معاه الرول أصلاً، مش محتاجين نعمل حاجة ونرجع نجاح
        if (await userManager.IsInRoleAsync(user, roleName))
            return Result.Success();

        var result = await userManager.AddToRoleAsync(user, roleName);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogWarning("فشل إضافة الدور {RoleName} للمستخدم {UserId}: {Errors}", roleName, userId, errors);
            return Result.Failure(UserErrors.UpdateFailed(errors)); // تأكد إنك ضايف Error للـ UpdateFailed أو استخدم واحد مناسب
        }

        logger.LogInformation("تم إضافة الدور {RoleName} للمستخدم {UserId} بنجاح", roleName, userId);
        return Result.Success();
    }

    public async Task<Result> RemoveRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Result.Failure(UserErrors.NotFound);

        // لو اليوزر معهوش الرول أصلاً، نرجع نجاح لأن الهدف متحقق
        if (!await userManager.IsInRoleAsync(user, roleName))
            return Result.Success();

        var result = await userManager.RemoveFromRoleAsync(user, roleName);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogWarning("فشل سحب الدور {RoleName} من المستخدم {UserId}: {Errors}", roleName, userId, errors);
            return Result.Failure(UserErrors.UpdateFailed(errors));
        }

        logger.LogInformation("تم سحب الدور {RoleName} من للمستخدم {UserId} بنجاح", roleName, userId);
        return Result.Success();
    }

    // ==========================================
    // الدوال المشتركة لكل المستخدمين (تمت إضافتها)
    // ==========================================

    public async Task<Result<bool>> CheckUserExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        return Result<bool>.Success(user != null);
    }

    public async Task<Result<bool>> IsEmailConfirmedAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
            return Result<bool>.Failure(UserErrors.NotFound); // تم تصحيح UserNotFound إلى NotFound

        return Result<bool>.Success(user.EmailConfirmed);
    }

    public async Task<Result> CreateUserAsync(string firstName, string lastName, string email, string password, CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            IsActive = true,
            EmailConfirmed = false
        };

        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
            return Result.Failure(UserErrors.CreationFailed(createResult.Errors.First().Description));

        // تعيين دور افتراضي للمستخدم العادي (الطالب)
        await userManager.AddToRoleAsync(user, "Student");

        return Result.Success();
    }

    public async Task<Result> ConfirmUserEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
            return Result.Failure(UserErrors.NotFound);

        user.EmailConfirmed = true;
        var updateResult = await userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
            return Result.Failure(UserErrors.UpdateFailed(updateResult.Errors.First().Description));

        return Result.Success();
    }
}