using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartRoutine.Application.Interfaces;
using SmartRoutine.Domain.Entities;
using SmartRoutine.Application.Common.Interfaces;

namespace SmartRoutine.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    // Basit in-memory blacklist (production için Redis ile değiştirilebilir)
    private static readonly HashSet<string> _refreshTokenBlacklist = new();
    private static readonly object _blacklistLock = new();
    private readonly ICacheService _cacheService;
    private const string RefreshTokenBlacklistPrefix = "refresh_blacklist:";
    
    public TokenService(IConfiguration configuration, ICacheService cacheService)
    {
        _configuration = configuration;
        _cacheService = cacheService;
        _secretKey = _configuration["Jwt:SecretKey"] ?? throw new ArgumentNullException("JWT SecretKey is required");
        _issuer = _configuration["Jwt:Issuer"] ?? "SmartRoutine.API";
        _audience = _configuration["Jwt:Audience"] ?? "SmartRoutine.Client";
    }

    public string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secretKey);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = _issuer,
            Audience = _audience
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public Guid? GetUserIdFromToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
        catch
        {
            return null;
        }
    }

    public bool ValidateToken(string token)
    {
        return GetUserIdFromToken(token) != null;
    }

    public async Task AddRefreshTokenToBlacklistAsync(string refreshToken)
    {
        if (_cacheService != null)
        {
            await _cacheService.SetAsync(RefreshTokenBlacklistPrefix + refreshToken, true, TimeSpan.FromDays(15));
        }
        else
        {
            lock (_blacklistLock)
            {
                _refreshTokenBlacklist.Add(refreshToken);
            }
        }
    }

    public async Task<bool> IsRefreshTokenBlacklistedAsync(string refreshToken)
    {
        if (_cacheService != null)
        {
            var result = await _cacheService.GetAsync<bool?>(RefreshTokenBlacklistPrefix + refreshToken);
            return result == true;
        }
        else
        {
            lock (_blacklistLock)
            {
                return _refreshTokenBlacklist.Contains(refreshToken);
            }
        }
    }

    public async Task RemoveRefreshTokenFromBlacklistAsync(string refreshToken)
    {
        if (_cacheService != null)
        {
            await _cacheService.RemoveAsync(RefreshTokenBlacklistPrefix + refreshToken);
        }
        else
        {
            lock (_blacklistLock)
            {
                _refreshTokenBlacklist.Remove(refreshToken);
            }
        }
    }

    public async Task ClearRefreshTokenBlacklistAsync()
    {
        if (_cacheService != null)
        {
            await _cacheService.RemoveByPatternAsync(RefreshTokenBlacklistPrefix);
        }
        else
        {
            lock (_blacklistLock)
            {
                _refreshTokenBlacklist.Clear();
            }
        }
    }
} 