using System.Globalization;
using Bankmore.Accounts.Query.Application.Abstraction;
using Bankmore.Accounts.Query.Application.Comon.Models;
using Bankmore.Accounts.Query.Infrastructure.Db;
using Microsoft.EntityFrameworkCore;

namespace Bankmore.Accounts.Query.Infrastructure.ReadStore;

public sealed class AccountsReadStore : IAccountsReadStore
{
    private readonly AccountsQueryDbContext _db;

    public AccountsReadStore(AccountsQueryDbContext db) => _db = db;

    public async Task<AccountInfo?> GetAccountAsync(int numeroConta, CancellationToken ct)
    {
        var acc = await _db.Contas
            .AsNoTracking()
            .Where(c => c.Numero == numeroConta)
            .Select(c => new AccountInfo
            {
                NumeroConta = c.Numero,
                Nome = c.Nome,
                Activa = c.Ativo
            })
            .FirstOrDefaultAsync(ct);

        return acc;
    }

    public async Task<BalanceProjection?> GetBalanceAsync(int accountId, bool includeTariffs, CancellationToken ct)
    {
        var movAgg = await _db.Movimentos
            .AsNoTracking()
            .Where(m => m.NumeroConta == accountId)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Credit = g.Where(m => m.TipoMovimento == "C").Sum(m => (decimal?)m.Valor) ?? 0m,
                Debit  = g.Where(m => m.TipoMovimento == "D").Sum(m => (decimal?)m.Valor) ?? 0m,
                Count  = g.Count(),
                MaxDateStr = g.Max(m => m.DataMovimento)
            })
            .FirstOrDefaultAsync(ct);

        decimal credit = movAgg?.Credit ?? 0m;
        decimal debit  = movAgg?.Debit  ?? 0m;
        int movCount   = movAgg?.Count  ?? 0;
        DateTime? maxDateStr = movAgg?.MaxDateStr;
        
        decimal totalTarifa = 0m;
        int tarifaCount = 0;
        DateTime? maxTarifaDateStr = null;

        if (includeTariffs)
        {
            var tarAgg = await _db.Tarifas
                .AsNoTracking()
                .Where(t => t.NumeroConta == accountId)
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Sum = g.Sum(t => (decimal?)t.Valor) ?? 0m,
                    Count = g.Count(),
                    MaxDateStr = g.Max(t => t.DataMovimento)
                })
                .FirstOrDefaultAsync(ct);

            totalTarifa = tarAgg?.Sum ?? 0m;
            tarifaCount = tarAgg?.Count ?? 0;
            maxTarifaDateStr = tarAgg?.MaxDateStr;
        }
        
        if (movAgg is null && (!includeTariffs || tarifaCount == 0))
        {
            return new BalanceProjection
            {
                LedgerBalance = 0m,
                AvailableBalance = 0m,
                Currency = "BRL",
                Version = 0,
                AsOfUtc = DateTime.UtcNow
            };
        }
        
        var ledger = credit - debit - (includeTariffs ? totalTarifa : 0m);
        var available = ledger;
        
        long version = movCount + (includeTariffs ? tarifaCount : 0);
        
        var asOfUtc = CoalesceMaxDateUtc(maxDateStr, maxTarifaDateStr) ?? DateTime.UtcNow;

        return new BalanceProjection
        {
            LedgerBalance = decimal.Round(ledger, 2),
            AvailableBalance = decimal.Round(available, 2),
            Currency = "BRL",
            Version = version,
            AsOfUtc = asOfUtc
        };
    }

    private static DateTime? CoalesceMaxDateUtc(DateTime? d1, DateTime? d2)
    {
        if (d1 == null && d2 == null)
            return null;
        if (d1 == null) return d2;
        if (d2 == null) return d1;
        return d1 > d2 ? d1 : d2;
    }
}