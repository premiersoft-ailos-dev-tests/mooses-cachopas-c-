namespace Bankmore.Accounts.Command.Domain.Accounts;

public sealed class CreateAccountModel
{
    public int Numero { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public bool Ativa { get; set; }
    public string Senha { get; set; } = string.Empty;
    public string Salt  { get; set; } = string.Empty;
    
    public CreateAccountModel(){}

    private CreateAccountModel(string nome, bool ativa, string cpf, string senha, string salt)
    {
        Nome = nome;
        Ativa = ativa;
        Cpf = cpf;
        Senha = senha;
        Salt = salt;
    }
    
    public static CreateAccountModel Create(string nome, string cpf, string senha, string salt) =>
        new(nome, ativa: true, cpf, senha, salt);
}