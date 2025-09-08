using Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Infra.Db.Services;

public class TransfersStore : ITransfersStore
{
    private readonly CommandDbContext _db;
    public TransfersStore(CommandDbContext db) => _db = db;

    public async Task<AccountValid> GetAccountAsync(int numeroConta, CancellationToken ct)
    {
        return await _db.Contas
            .AsNoTracking()
            .Where(c => c.Numero == numeroConta)
            .Select(c => new AccountValid(true, c.Numero, c.Nome, c.Ativa))
            .FirstOrDefaultAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
    
    public async Task<int> AddTransferAsync(int origemId, int destinoId, DateTime dataMov, decimal valor, CancellationToken ct)
    {
        try
        {
            _db.Transferencias.Add(new Db.Entities.TransfersDbModel
            {
                IdContaCorrenteOrigem = origemId,
                IdContaCorrenteDestino = destinoId,
                DataMovimento = dataMov,
                Valor = valor
            });
            var result = await _db.SaveChangesAsync(ct);
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }
}