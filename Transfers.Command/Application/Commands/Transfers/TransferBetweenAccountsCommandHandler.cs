using System.Text.Json;
using Application.Abstractions;
using Application.Services;
using Infra.Contracts;
using Infra.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.Transfers;

public sealed class TransferBetweenAccountsCommandHandler
    : IRequestHandler<TransferBetweenAccountsCommand, TransferBetweenAccountsResult>
{
    private readonly IClock _clock;
    private readonly IIdempotencyStore _idemp;
    private readonly ILogger<TransferBetweenAccountsCommandHandler> _logger;
    private readonly IMovmentsApi _movmentsApi;
    private readonly ITransfersStore _transfersStore;

    public TransferBetweenAccountsCommandHandler(
        IMovmentsApi movmentsApi,
        ITransfersStore transactionStore,
        ITransfersStore transfersStore,
        IClock clock,
        IIdempotencyStore idemp,
        ILogger<TransferBetweenAccountsCommandHandler> logger)
    {
        _clock = clock;
        _idemp = idemp;
        _movmentsApi = movmentsApi;
        _logger = logger;
        _transfersStore = transfersStore;
    }

    public async Task<TransferBetweenAccountsResult> Handle(TransferBetweenAccountsCommand req, CancellationToken ct)
    {
        var savedJson = await _idemp.GetResultAsync(req.IdempotencyKey, ct);
        if (!string.IsNullOrWhiteSpace(savedJson))
        {
            var cached = JsonSerializer.Deserialize<TransferBetweenAccountsResult>(savedJson)!;
            return cached;
        }

        var origem = await _transfersStore.GetAccountAsync(req.OrigemAccountId, ct);
        if (!origem.Exists)
            return await SaveOnlyResultAsync(req, Fail("INVALID_ACCOUNT", "Conta de origem não encontrada."), ct);
        if (!origem.Ativa)
            return await SaveOnlyResultAsync(req, Fail("INACTIVE_ACCOUNT", "Conta de origem inativa."), ct);

        var destino = await _transfersStore.GetAccountAsync(req.DestinoAccountId, ct);
        if (!destino.Exists)
            return await SaveOnlyResultAsync(req, Fail("INVALID_ACCOUNT", "Conta de destino não encontrada."), ct);
        if (!destino.Ativa)
            return await SaveOnlyResultAsync(req, Fail("INACTIVE_ACCOUNT", "Conta de destino inativa."), ct);

        var agoraUtc = _clock.UtcNow;

        var requisicaoDebito = new MovmentRequest(req.OrigemAccountId, req.Valor, "D");
        var requisicaoCredito = new MovmentRequest(req.DestinoAccountId, req.Valor, "C");

        try
        {
            var respDebito = await _movmentsApi.MovmentAsync(requisicaoDebito, req.Token, Guid.NewGuid().ToString(), ct);
            if (!respDebito.IsSuccessStatusCode)
                return await SaveOnlyResultAsync(req, Fail("REMOTE_ERROR", "Falha ao debitar conta de origem."), ct);

            try
            {
                var respCredito = await _movmentsApi.MovmentAsync(requisicaoCredito, req.Token, Guid.NewGuid().ToString(), ct);
                if (!respCredito.IsSuccessStatusCode)
                {
                    requisicaoDebito.tipoOperacao = "C";
                    await _movmentsApi.MovmentAsync(requisicaoDebito, req.Token, Guid.NewGuid().ToString(), ct);
                    return await SaveOnlyResultAsync(req, Fail("REMOTE_ERROR", "Falha ao creditar conta de destino."),
                        ct);
                }
            }
            catch (Refit.ApiException apiEx)
            {
                _logger.LogError(apiEx, "Erro ao creditar destino; efetuando rollback do débito.");
                requisicaoDebito.tipoOperacao = "C";
                await _movmentsApi.MovmentAsync(requisicaoDebito, req.Token, Guid.NewGuid().ToString(), ct);
                return await SaveOnlyResultAsync(req, Fail("REMOTE_ERROR", "Erro ao creditar conta de destino."), ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao creditar destino; efetuando rollback do débito.");
                requisicaoDebito.tipoOperacao = "C";
                await _movmentsApi.MovmentAsync(requisicaoDebito, req.Token, Guid.NewGuid().ToString(), ct);
                return await SaveOnlyResultAsync(req, Fail("REMOTE_ERROR", "Erro ao creditar conta de destino."), ct);
            }

            try
            {
                await _transfersStore.AddTransferAsync(req.OrigemAccountId, req.DestinoAccountId, agoraUtc, req.Valor,
                    ct);
                await _transfersStore.SaveChangesAsync(ct);

                var ok = Success();
                await SaveOnlyResultAsync(req, ok, ct);
                return ok;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao persistir transferência; efetuando rollback total.");

                requisicaoDebito.tipoOperacao = "C";
                requisicaoCredito.tipoOperacao = "D";
                try
                {
                    await _movmentsApi.MovmentAsync(requisicaoDebito, req.Token, Guid.NewGuid().ToString(), ct);
                    await _movmentsApi.MovmentAsync(requisicaoCredito, req.Token, Guid.NewGuid().ToString(), ct);
                }
                catch (Exception rbEx)
                {
                    _logger.LogError(rbEx, "Falha durante rollback de movimentos.");
                }

                return await SaveOnlyResultAsync(req, Fail("PERSISTENCE_ERROR", "Falha ao persistir a transferência."),
                    ct);
            }
        }
        catch (Refit.ApiException apiEx)
        {
            _logger.LogError(apiEx, "Erro inesperado (Refit) durante a transferência.");
            return await SaveOnlyResultAsync(req, Fail("REMOTE_ERROR", apiEx.Message), ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado durante a transferência.");
            return await SaveOnlyResultAsync(req, Fail("UNEXPECTED_ERROR", ex.Message), ct);
        }
    }

    private static TransferBetweenAccountsResult Success()
        => new(true, null, null);

    private static TransferBetweenAccountsResult Fail(string type, string message)
        => new(false, type, message);

    private async Task<TransferBetweenAccountsResult> SaveOnlyResultAsync(
        TransferBetweenAccountsCommand req,
        TransferBetweenAccountsResult res,
        CancellationToken ct)
    {
        await _idemp.SaveAsync(
            req.IdempotencyKey,
            string.Empty,
            JsonSerializer.Serialize(res),
            ct);

        return res;
    }
}
