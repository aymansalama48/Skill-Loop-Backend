using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Application.Common.Abstractions.Identity.Tokens;
using Skill_Loop.Domain.Constants;
using Skill_Loop.Infrastructure.Identity.Tokens;   // حسب مكان IJwtTokenGenerator عندك
using Skill_Loop.Infrastructure.Persistence.IdentityModels;

namespace Skill_Loop.Api.Controllers.Dev;

/// <summary>
/// Controller للتطوير فقط — بيسمح بإنشاء توكن بسرعة بدون المرور بـ OTP/2FA.
/// ⚠️ في بيئة Production، كل الـ Endpoints دي بترجع 404.
/// </summary>
[ApiController]
[Route("api/v1/dev")]
[AllowAnonymous]
public class DevController : BaseApiController
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenGenerator _jwtGenerator;
    private readonly IWebHostEnvironment _env;

    public DevController(
        UserManager<ApplicationUser> userManager,
        IJwtTokenGenerator jwtGenerator,
        IWebHostEnvironment env)
    {
        _userManager = userManager;
        _jwtGenerator = jwtGenerator;
        _env = env;
    }

    /// <summary>
    /// تسجيل دخول سريع للتطوير — يرجّع توكن جاهز للإيميل المطلوب.
    /// الاستخدام: POST /api/v1/dev/quick-login?email=admin@skillloop.com
    /// </summary>
    [HttpPost("quick-login")]
    public async Task<IResult> QuickLogin(
        CancellationToken cancellationToken)
    {
        // 🛡️ حماية: اشتغل في Development فقط
        if (!_env.IsDevelopment())
            return Results.NotFound();

        string email = "admin@skillloop.com";
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return Results.NotFound(new { message = $"المستخدم {email} غير موجود." });

        var roles = await _userManager.GetRolesAsync(user);

        var accessToken = _jwtGenerator.GenerateJwtToken(
            user.Id,
            user.Email!,
            user.FullName,
            roles,
            []);

        // ملاحظة: مش بنولّد RefreshToken عشان نخلي الـ Endpoint نضيف
        return Results.Ok(new
        {
            userId = user.Id,
            email = user.Email,
            fullName = user.FullName,
            roles = roles,
            accessToken = accessToken,
            expiresInSeconds = 3600,
            hint = "استخدم accessToken ده في Scalar → Auth → Bearer"
        });
    }

    /// <summary>
    /// قائمة بكل المستخدمين المتاحين للتجربة (للاختيار السريع)
    /// </summary>
    [HttpGet("users")]
    public async Task<IResult> GetDevUsers(CancellationToken cancellationToken)
    {
        if (!_env.IsDevelopment())
            return Results.NotFound();

        var users = _userManager.Users
            .Select(u => new { u.Id, u.Email, u.FullName, u.IsActive })
            .Take(50)
            .ToList();

        return Results.Ok(users);
    }
}