namespace Bankmore.Accounts.Command.Api.Contracts;
public sealed class ActivateAccountRequest
{
    public int? NumeroConta { get; set; }
    public string Password { get; set; } = string.Empty;
}