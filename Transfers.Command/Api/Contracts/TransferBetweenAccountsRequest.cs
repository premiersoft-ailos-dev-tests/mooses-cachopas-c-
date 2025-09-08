namespace Api.Contracts;

public sealed record TransferBetweenAccountsRequest
(
    int OrigemAccountId,
    int DestinoAccountId,
    decimal Valor
);