using Bankmore.Accounts.Command.Application.Abstractions;
using Bankmore.Accounts.Command.Domain.Extensions.Movments;
using Bankmore.Accounts.Command.Domain.Models.Movments;
using Bankmore.Accounts.Command.Infrastructure.Db;
using Microsoft.EntityFrameworkCore;

namespace Bankmore.Accounts.Command.Infrastructure.Services;

public sealed class TransactionsStore : ITransactionsStore
{
    private readonly CommandDbContext _db;
    public TransactionsStore(CommandDbContext db) => _db = db;

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);

    public async Task<bool> RegisterMovment(MovmentModel movmentModel, CancellationToken ct)
    {
        _db.Movimentos.Add(new Db.Entities.MovmentsDbModel
        {
            IdContaCorrente = movmentModel.IdConta,
            DataMovimento = movmentModel.Data,
            TipoMovimento =  movmentModel.MovmentType.ToCode(),
            Valor = movmentModel.Valor
        });
        await Task.CompletedTask;
        return true;
    }

    public async Task<decimal> GetSaldoAtualAsync(int numeroConta, CancellationToken ct)
    {
        var credit = await _db.Movimentos.Where(m => m.IdContaCorrente == numeroConta && m.TipoMovimento == "C")
                        .SumAsync(m => (decimal?)m.Valor, ct) ?? 0m;
        var debit = await _db.Movimentos.Where(m => m.IdContaCorrente == numeroConta && m.TipoMovimento == "D")
                        .SumAsync(m => (decimal?)m.Valor, ct) ?? 0m;
        var fees = await _db.Tarifas.Where(t => t.IdContaCorrente == numeroConta)
                        .SumAsync(t => (decimal?)t.Valor, ct) ?? 0m;
        return credit - debit - fees;
    }
}