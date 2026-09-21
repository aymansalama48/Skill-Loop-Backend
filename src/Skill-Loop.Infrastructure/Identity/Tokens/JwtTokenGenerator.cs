using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Skill_Loop.Application.Common.Abstractions.Core;
using Skill_Loop.Application.Common.Abstractions.Identity.Tokens;
using Skill_Loop.Application.Common.Constants;
using Skill_Loop.Infrastructure.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Skill_Loop.Infrastructure.Identity.Tokens;

/// <summary>
/// مولّد توكنات JWT — للمريض والـ Staff
/// </summary>
public class JwtTokenGenerator(
    IOptions<JwtOptions> options,
    IDateTime dateTime) : IJwtTokenGenerator
{



    public string GenerateJwtToken(
        Guid userId,
        string email,
        string fullName,
        IEnumerable<string> roles,
        IEnumerable<string> permissions)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(CustomClaims.UserId, userId.ToString()),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Name, fullName)
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        foreach (var permission in permissions)
            claims.Add(new Claim(CustomClaims.Permission, permission));



        return BuildToken(claims, options.Value.ExpiryMinutes);
    }

    // بناء التوكن النهائي بالـ Claims ومدة الصلاحية
    private string BuildToken(List<Claim> claims, int expiryMinutes)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Value.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: options.Value.Issuer,
            audience: options.Value.Audience,
            claims: claims,
            expires: dateTime.Now.AddMinutes(expiryMinutes), // أو dateTime.UtcNow
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}