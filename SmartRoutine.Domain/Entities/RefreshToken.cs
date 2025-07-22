using SmartRoutine.Domain.Common;

namespace SmartRoutine.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTime Expires { get; private set; }
    public DateTime Created { get; private set; }
    public string? CreatedByIp { get; private set; }
    public DateTime? Revoked { get; private set; }
    public string? RevokedByIp { get; private set; }
    public string? ReplacedByToken { get; private set; }
    public bool IsActive => Revoked == null && !IsExpired;
    public bool IsExpired => DateTime.UtcNow >= Expires;

    // Navigation
    public virtual User User { get; private set; } = null!;

    private RefreshToken() { }

    public RefreshToken(Guid userId, string token, DateTime expires, string? createdByIp)
    {
        UserId = userId;
        Token = token;
        Expires = expires;
        Created = DateTime.UtcNow;
        CreatedByIp = createdByIp;
    }

    public void Revoke(string? revokedByIp, string? replacedByToken = null)
    {
        Revoked = DateTime.UtcNow;
        RevokedByIp = revokedByIp;
        ReplacedByToken = replacedByToken;
    }
} 