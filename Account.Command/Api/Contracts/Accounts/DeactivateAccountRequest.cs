namespace Bankmore.Accounts.Command.Api.Contracts;

public sealed class DeactivateAccountRequest
{
    public int? NumeroConta { get; set; } 
    public string Password { get; set; } = string.Empty;
}