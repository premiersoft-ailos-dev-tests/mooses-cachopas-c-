namespace Infra.Models;

public record LoginResponse
{
    public string accessToken { get; set; } = string.Empty;
    public DateTime expiresAtUtc { get; set; }
    public string tokenType { get; set; } = string.Empty;
}