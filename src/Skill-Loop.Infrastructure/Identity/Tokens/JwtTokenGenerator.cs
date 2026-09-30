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
        // Fail fast rather than sign with a missing/short key. HMAC-SHA256 requires a key
        // of at least 256 bits; anything shorter silently weakens the signature.
        var keyMaterial = options.Value.Key;

        if (string.IsNullOrWhiteSpace(keyMaterial) ||
            System.Text.Encoding.UTF8.GetByteCount(keyMaterial) < 32)
        {
            throw new InvalidOperationException(
                "Jwt:Key must be supplied (environment variable or user-secrets) and be at " +
                "least 32 bytes. Refusing to sign tokens with a weak or missing key.");
        }

        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(keyMaterial));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // RFC 7519 numeric-date claims (exp, nbf, iat) are UTC seconds since the epoch.
        // The old code passed `dateTime.Now`, which returns Egypt local time — so every
        // token's real lifetime was shifted by the UTC offset, and by an extra hour during
        // Egypt DST. `UtcNow` is the only correct value here.
        var issuedAt = dateTime.UtcNow;
        var expires = issuedAt.AddMinutes(expiryMinutes);

        // `iat` is added as an explicit claim: the JwtSecurityToken constructor overload that
        // takes it is not available in all supported versions of the token library, and
        // omitting `iat` loses the audit signal that the token was minted at a known time.
        if (!claims.Exists(c => c.Type == JwtRegisteredClaimNames.Iat))
        {
            claims.Add(new Claim(
                JwtRegisteredClaimNames.Iat,
                EpochTime.GetIntDate(issuedAt).ToString(),
                ClaimValueTypes.Integer64));
        }

        var token = new JwtSecurityToken(
            issuer: options.Value.Issuer,
            audience: options.Value.Audience,
            claims: claims,
            notBefore: issuedAt,
            expires: expires,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}