namespace Skill_Loop.Infrastructure.Persistence.IdentityModels;

/// <summary>
/// A persisted refresh token.
///
/// Security: refresh tokens are bearer credentials valid for days, so only a SHA-256
/// digest is stored. <see cref="TokenHash"/> holds the digest; the plaintext token is
/// returned to the client once and never persisted. Without this, a read-only database
/// compromise (SQL injection, leaked backup, over-privileged admin) yields immediately
/// usable sessions for every user, including SuperAdmin.
///
/// Migration note: existing rows are invalidated by the Token column -&gt; TokenHash
/// migration, so all users must re-authenticate once.
/// </summary>
public class RefreshToken
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }  // المفتاح الخارجي للمستخدم

    /// <summary>SHA-256 (Base64) digest of the opaque token. Never the token itself.</summary>
    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiryDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public bool IsRevoked { get; set; } = false;

    /// <summary>
    /// Groups all tokens descended from a single login, so reuse of an already-rotated
    /// token can revoke the entire family (the standard signal of token theft).
    /// </summary>
    public Guid TokenFamilyId { get; set; }

    /// <summary>
    /// Set when this token is rotated. Presenting it afterwards means the token leaked and
    /// the whole family must be revoked.
    /// </summary>
    public string? ReplacedByTokenHash { get; set; }

    // Navigation Property (اختياري)
    public virtual ApplicationUser User { get; set; } = null!;
}
