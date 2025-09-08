namespace Bankmore.Accounts.Command.Infrastructure.Db.Entities;

public sealed class IdempotenciaDbModel
{
    public string ChaveIdempotencia { get; set; } = default!;
    public string? Requisicao { get; set; }
    public string? Resultado { get; set; }
}