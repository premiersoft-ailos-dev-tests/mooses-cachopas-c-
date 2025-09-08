using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Primitives;

namespace Bankmore.Shared.Utils;

public static class TokenUtils
{
    public static string? GetAccountIdFromRawJwt(string jwt)
    {
        if (string.IsNullOrWhiteSpace(jwt)) return null;

        var raw = jwt.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? jwt.Substring("Bearer ".Length).Trim()
            : jwt.Trim();

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(raw); 

        return token.Claims.FirstOrDefault(c =>
                   c.Type == ClaimTypes.NameIdentifier
                || c.Type == "nameid"
                || c.Type == "sub")
               ?.Value;
    }

    public static string ExtractToken(StringValues authorizationHeader)
    {
        var raw = authorizationHeader.ToString();

        if (string.IsNullOrWhiteSpace(raw))
            return string.Empty;

        return raw.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? raw.Substring("Bearer ".Length).Trim()
            : raw.Trim();
    }
    
    public static string ExtractFullToken(StringValues authorizationHeader)
    {
        var raw = authorizationHeader.ToString();

        if (string.IsNullOrWhiteSpace(raw))
            return string.Empty;

        return raw;
    }
}