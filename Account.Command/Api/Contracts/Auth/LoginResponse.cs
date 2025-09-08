namespace Bankmore.Accounts.Command.Api.Contracts;

public sealed class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public string TokenType { get; set; } = "Bearer";
    public string AccountId { get; set; } = string.Empty;
}