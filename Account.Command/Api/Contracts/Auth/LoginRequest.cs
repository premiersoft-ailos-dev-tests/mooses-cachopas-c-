namespace Bankmore.Accounts.Command.Api.Contracts;

public sealed class LoginRequest
{
    public int? Numero { get; set; } 
    public string? Cpf { get; set; }
    public string Senha { get; set; } = string.Empty;
}