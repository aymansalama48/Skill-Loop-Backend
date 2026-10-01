using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Application.Common.Abstractions.Identity.Tokens;
using Skill_Loop.Domain.Constants;
using Skill_Loop.Infrastructure.Identity.Tokens;   // ��� ���� IJwtTokenGenerator ����
using Skill_Loop.Infrastructure.Persistence.IdentityModels;
namespace Skill_Loop.Api.Controllers.Dev;
/// <summary>
/// أدوات وبيانات تجريبية للمطورين (لبيئة التطوير فقط)
/// </summary>
[ApiController]
[Route("api/v1/dev")]
[Authorize]
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
    [HttpPost("quick-login")]
    [AllowAnonymous]
    public async Task<IResult> QuickLogin(
        CancellationToken cancellationToken)
    {
        // ??? �����: ����� �� Development ���
        if (!_env.IsDevelopment())
            return Results.NotFound();
        string email = "admin@skillloop.com";
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return Results.NotFound(new { message = $"�������� {email} ��� �����." });
        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtGenerator.GenerateJwtToken(
            user.Id,
            user.Email!,
            user.FullName,
            roles,
            []);
        // ������: �� ������ RefreshToken ���� ���� ��� Endpoint ����
        return Results.Ok(new
        {
            userId = user.Id,
            email = user.Email,
            fullName = user.FullName,
            roles = roles,
            accessToken = accessToken,
            expiresInSeconds = 3600,
            hint = "������ accessToken �� �� Scalar ? Auth ? Bearer"
        });
    }
    [HttpGet("users")]
    [AllowAnonymous]
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