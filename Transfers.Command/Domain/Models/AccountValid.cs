namespace Domain.Models;

public readonly record struct AccountValid(
    bool Exists,
    int Numero,
    string Nome,
    bool Ativa
);