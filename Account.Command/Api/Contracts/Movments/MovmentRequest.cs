namespace Bankmore.Accounts.Command.Api.Contracts.Transactions;

public sealed record MovmentRequest(int? NumeroConta, decimal Valor, string TipoOperacao);