using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using AutoMapper;
using Bankmore.Accounts.Command.Application.Abstractions;
using Bankmore.Accounts.Command.Application.Common.Exceptions;
using Bankmore.Accounts.Command.Domain.Enums.Movments;
using Bankmore.Accounts.Command.Domain.Models.Movments;
using Bankmore.Shared.Utils;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bankmore.Accounts.Command.Application.Commands.Transactions.Movments;

public class MovmentHandler : IRequestHandler<MovmentCommand, MovmentResult>
{
    private readonly ITransactionsService _transactionsService;
    private readonly ILogger<MovmentHandler> _logger;
    private readonly IAccountsService _accountsService;
    private readonly IMapper _mapper;
    private readonly IIdempotencyStore _idemp;
    private readonly IClock _clock;

    public MovmentHandler(
        ITransactionsService transactionsService,
        IAccountsService accountsService,
        IMapper mapper,
        IClock clock,
        IIdempotencyStore idemp,
        ILogger<MovmentHandler> logger)
    {
        _transactionsService = transactionsService;
        _logger = logger;
        _accountsService = accountsService;
        _mapper = mapper;
        _idemp = idemp;
        _clock = clock;
    }

    public async Task<MovmentResult> Handle(MovmentCommand request, CancellationToken cancellationToken)
    {
        var savedJson = await _idemp.GetResultAsync(request.IdempotencyKey, cancellationToken);
        if (!string.IsNullOrWhiteSpace(savedJson))
        {
            var cached = JsonSerializer.Deserialize<MovmentResult>(savedJson)!;
            return cached;
        }

        try
        {
            var tokenAccountString = TokenUtils.GetAccountIdFromRawJwt(request.Token);
            var tokenAccount = Convert.ToInt32(tokenAccountString);

            var movmentModel = _mapper.Map<MovmentModel>(request);

            if (request.IdConta == null)
                movmentModel.IdConta = tokenAccount;

            if (movmentModel.IdConta <= 0)
                throw new BusinessRuleException("Numero de Conta Inválido");

            var accountExists = await _accountsService.GetAccountAsync(movmentModel.IdConta, cancellationToken);

            if (!accountExists.Exists)
                throw new BusinessRuleException("Conta não encontrada.");

            if (!accountExists.Ativa)
                throw new BusinessRuleException("Conta não esta ativa.");

            if (movmentModel.MovmentType == MovmentType.Default)
                throw new BusinessRuleException("Tipo de movimentação inválido.");

            if (movmentModel.MovmentType == MovmentType.Debit)
            {
                if (tokenAccount != movmentModel.IdConta)
                    throw new BusinessRuleException("Conta não possui permissão para realizar essa operação.");

                var saldo = await _transactionsService.GetSaldoAtualAsync(movmentModel.IdConta, cancellationToken);
                if (saldo < movmentModel.Valor)
                    throw new BusinessRuleException("Conta não possui saldo suficiente.");
            }

            await _transactionsService.RegisterMovment(movmentModel, cancellationToken);
            await _transactionsService.SaveChangesAsync(cancellationToken);

            var ok = new MovmentResult(true);

            await _idemp.SaveAsync(
                request.IdempotencyKey,
                requestJson: string.Empty,                          
                resultJson: JsonSerializer.Serialize(ok),
                cancellationToken);

            return ok;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Erro inesperado ao realizar a operação de {Valor} na conta {IdConta}.",
                request.Valor, request.IdConta);

            throw;
        }
    }
}