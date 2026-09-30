using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Skill_Loop.Application.Common.Abstractions.Core;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Abstractions.Identity.Invitations;
using Skill_Loop.Application.Common.Abstractions.Identity.Providers;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Identity;
using Skill_Loop.Application.Common.Errors.Invitations;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Invitation;
using Skill_Loop.Infrastructure.Persistence.IdentityModels;
using System.Security.Cryptography;

namespace Skill_Loop.Infrastructure.Identity.Invitations;

public sealed class InvitationService(
    IApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    ICurrentUser currentUser,
    IDateTime dateTime,
    IEnumerable<IExternalAuthProvider> externalAuthProviders,
    ILogger<InvitationService> logger) : IInvitationService
{
    public async Task<Result<string>> SendStaffInvitationAsync(
        string email,
        string role,
        CancellationToken cancellationToken = default)
    {
        // 1. التحقق من وجود الحساب مسبقاً
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser is not null)
            return Result<string>.Failure(UserErrors.EmailAlreadyExists);

        var now = dateTime.UtcNow;

        // 2. إبطال الدعوات المعلقة القديمة لنفس البريد
        var pendingInvitations = await context.StaffInvitations
            .Where(i => i.Email == email && !i.IsUsed && i.ExpiresAtUtc > now)
            .ToListAsync(cancellationToken);

        foreach (var pending in pendingInvitations)
        {
            pending.ExpiresAtUtc = now;
        }

        // 3. إنشاء التوكن والكيان عبر الـ Factory Method
        var token = GenerateSecureToken();
        var invitation = StaffInvitation.Create(
            email: email,
            role: role,
            token: token,
            expiresAtUtc: now.AddDays(2),
            adminId: currentUser.UserId ?? Guid.Empty,
            adminName: currentUser.FullName ?? "System Admin");

        context.Add(invitation);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("تم إنشاء سجل الدعوة بنجاح للبريد {Email}", email);

        return Result<string>.Success(token);
    }

    public async Task<Result<InvitationDetailsDto>> ValidateInvitationTokenAsync(
        string invitationToken,
        CancellationToken cancellationToken = default)
    {
        var invitation = await context.StaffInvitations
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Token == invitationToken, cancellationToken);

        if (invitation is null)
            return Result<InvitationDetailsDto>.Failure(InvitationErrors.NotFound);

        if (invitation.IsUsed)
            return Result<InvitationDetailsDto>.Failure(InvitationErrors.AlreadyUsed);

        if (invitation.ExpiresAtUtc <= dateTime.UtcNow)
            return Result<InvitationDetailsDto>.Failure(InvitationErrors.Expired);

        var details = new InvitationDetailsDto(
            InvitationId: invitation.Id,
            Email: invitation.Email,
            Role: invitation.Role,
            AdminName: invitation.AdminName,
            IsValid: true);

        return Result<InvitationDetailsDto>.Success(details);
    }

    public async Task<Result> RevokeInvitationAsync(
        Guid invitationId,
        CancellationToken cancellationToken = default)
    {
        var invitation = await context.StaffInvitations
            .FirstOrDefaultAsync(i => i.Id == invitationId, cancellationToken);

        if (invitation is null)
            return Result.Failure(InvitationErrors.NotFound);

        if (invitation.IsUsed)
            return Result.Failure(InvitationErrors.AlreadyUsed);

        invitation.ExpiresAtUtc = dateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("تم إبطال الدعوة {InvitationId}", invitationId);

        return Result.Success();
    }

    public async Task<Result<bool>> AcceptInvitationAndCreateAccountAsync(
        string invitationToken,
        string fullName,
        string password,
        string? phoneNumber = null,
        CancellationToken cancellationToken = default)
    {
        var invitation = await context.StaffInvitations
            .FirstOrDefaultAsync(i => i.Token == invitationToken, cancellationToken);

        if (invitation is null)
            return Result<bool>.Failure(InvitationErrors.NotFound);

        if (invitation.IsUsed || invitation.ExpiresAtUtc <= dateTime.UtcNow)
            return Result<bool>.Failure(InvitationErrors.InvalidOrExpired);

        // 1. تقسيم الاسم الكامل إلى أجزائه
        var nameParts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var firstName = nameParts.Length > 0 ? nameParts[0] : fullName;
        var lastName = nameParts.Length > 1 ? nameParts[^1] : string.Empty;

        // 2. إنشاء المستخدم الأساسي
        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            UserName = invitation.Email,
            Email = invitation.Email,
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phoneNumber,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            var details = string.Join(", ", createResult.Errors.Select(e => e.Description));
            return Result<bool>.Failure(UserErrors.CreationFailed(details));
        }

        // 3. إضافة الدور للمستخدم
        var roleResult = await userManager.AddToRoleAsync(user, invitation.Role);
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return Result<bool>.Failure(UserErrors.CreationFailed("فشل تعيين الدور الوظيفي للمستخدم."));
        }

        // إغلاق الدعوة
        invitation.IsUsed = true;
        invitation.UsedAtUtc = dateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("تم إكمال قبول الدعوة وتفعيل حساب {Email} بالطريقة التقليدية", user.Email);

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> AcceptInvitationWithGoogleAsync(
        string invitationToken,
        string googleIdToken,
        CancellationToken cancellationToken = default)
    {
        var invitation = await context.StaffInvitations
            .FirstOrDefaultAsync(i => i.Token == invitationToken, cancellationToken);

        if (invitation is null)
            return Result<bool>.Failure(InvitationErrors.NotFound);

        if (invitation.IsUsed || invitation.ExpiresAtUtc <= dateTime.UtcNow)
            return Result<bool>.Failure(InvitationErrors.InvalidOrExpired);

        var googleProvider = externalAuthProviders.FirstOrDefault(p => p.ProviderName == "Google");
        if (googleProvider is null)
            return Result<bool>.Failure(UserErrors.ValidationFailed("مزود خدمة جوجل غير مفعل."));

        var googleTokenResult = await googleProvider.ValidateTokenAsync(googleIdToken, cancellationToken);
        if (!googleTokenResult.IsSuccess)
            return Result<bool>.Failure(googleTokenResult.Errors);

        var googleUser = googleTokenResult.Data!;

        if (!string.Equals(googleUser.Email, invitation.Email, StringComparison.OrdinalIgnoreCase))
        {
            return Result<bool>.Failure(UserErrors.ValidationFailed("البريد الإلكتروني لحساب جوجل لا يطابق البريد الإلكتروني الموجهة له الدعوة."));
        }

        var nameParts = googleUser.FullName?.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? [];
        var firstName = nameParts.Length > 0 ? nameParts[0] : googleUser.Email;
        var lastName = nameParts.Length > 1 ? nameParts[^1] : string.Empty;

        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            UserName = invitation.Email,
            Email = invitation.Email,
            FirstName = firstName,
            LastName = lastName,
            AvatarUrl = googleUser.AvatarUrl,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(user);
        if (!createResult.Succeeded)
        {
            var details = string.Join(", ", createResult.Errors.Select(e => e.Description));
            return Result<bool>.Failure(UserErrors.CreationFailed(details));
        }

        var loginInfo = new UserLoginInfo("Google", googleUser.ProviderUserId, "Google");
        var addLoginResult = await userManager.AddLoginAsync(user, loginInfo);

        if (!addLoginResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return Result<bool>.Failure(UserErrors.CreationFailed("فشل ربط الحساب بجوجل."));
        }

        var roleResult = await userManager.AddToRoleAsync(user, invitation.Role);
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return Result<bool>.Failure(UserErrors.CreationFailed("فشل تعيين الدور الوظيفي للمستخدم."));
        }

        invitation.IsUsed = true;
        invitation.UsedAtUtc = dateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("تم إكمال قبول الدعوة وتفعيل حساب {Email} بنجاح عبر حساب Google", user.Email);

        return Result<bool>.Success(true);
    }

    private static string GenerateSecureToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToHexString(randomBytes).ToLowerInvariant();
    }
}