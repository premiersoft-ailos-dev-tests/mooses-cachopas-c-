namespace Infra.Models;

public record LoginRequest
{
    public LoginRequest(int numero, string senha)
    {
        this.numero = numero;
        this.senha = senha;
    }
    public int numero { get; set; }
    public string senha { get; set; } = string.Empty;
}