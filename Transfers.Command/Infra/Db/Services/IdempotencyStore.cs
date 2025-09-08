using Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Infra.Db.Services;

public sealed class IdempotencyStore : IIdempotencyStore
{
    private readonly CommandDbContext _db;
    
    public IdempotencyStore(CommandDbContext db) => _db = db;
    public async Task<string?> GetResultAsync(string key, CancellationToken ct)
        => await _db.Idempotencias.AsNoTracking()
            .Where(i => i.ChaveIdempotencia == key)
            .Select(i => i.Resultado)
            .FirstOrDefaultAsync(ct);

    public async Task SaveAsync(string key, string requestJson, string resultJson, CancellationToken ct)
    {
        _db.Idempotencias.Add(new Db.Entities.IdempotenciaDbModel
        {
            ChaveIdempotencia = key,
            Requisicao = requestJson,
            Resultado  = resultJson
        });
        await _db.SaveChangesAsync(ct);
    }
}