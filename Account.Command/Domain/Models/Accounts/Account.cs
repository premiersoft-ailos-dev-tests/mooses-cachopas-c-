namespace Bankmore.Accounts.Command.Domain.Accounts;

public sealed class Account
{
    public string Id { get; }
    public int Numero { get; }
    public string Nome { get; private set; }
    public bool Ativa { get; private set; }
    public string Cpf { get; private set; }

    private Money _saldo = Money.Zero;

    private Account(string id, int numero, string nome, bool ativa, string cpf)
    {
        Id = id; Numero = numero; Nome = nome; Ativa = ativa; cpf = cpf;
    }
    
    private Account(string id, string nome, bool ativa, string cpf)
    {
        Id = id; 
        Nome = nome; 
        Ativa = ativa; 
        Cpf = cpf;
    }

    public static Account Create(string id,  string nome, string cpf) =>
        new(id, nome, ativa: true, cpf);

    public void EnsureActive()
    {
        if (!Ativa) throw new InvalidOperationException("Conta inativa.");
    }

    public void Debit(Money valor)
    {
        EnsureActive();
        if (valor.Value <= 0) throw new ArgumentOutOfRangeException(nameof(valor));
        if (_saldo.Value - valor.Value < 0) throw new InvalidOperationException("Saldo insuficiente.");
        _saldo -= valor;
    }

    public void Credit(Money valor)
    {
        EnsureActive();
        if (valor.Value <= 0) throw new ArgumentOutOfRangeException(nameof(valor));
        _saldo += valor;
    }
}