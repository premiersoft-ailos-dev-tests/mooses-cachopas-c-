using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Bankmore.Accounts.Query.Api.Security;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "bankmore.auth";
    public string Audience { get; set; } = "bankmore.api";
    public string SigningKey { get; set; } = "";
    public int ExpiresMinutes { get; set; } = 60;
}

public interface IJwtTokenService
{
    (string token, DateTime expiresAtUtc) CreateForAccount(string accountId);
}

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _opt;
    private readonly SigningCredentials _creds;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _opt = options.Value;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.SigningKey));
        _creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }

    public (string token, DateTime expiresAtUtc) CreateForAccount(string accountId)
    {
        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(_opt.ExpiresMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, accountId),
            new Claim("scope", "accounts.read accounts.write"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

        var jwt = new JwtSecurityToken(
            issuer: _opt.Issuer,
            audience: _opt.Audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: _creds);

        var token = new JwtSecurityTokenHandler().WriteToken(jwt);
        return (token, expires);
    }
}