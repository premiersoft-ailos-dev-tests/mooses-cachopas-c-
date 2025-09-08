using System.Security.Cryptography.X509Certificates;

public readonly record struct AccountValid(
    bool Exists,
    int Numero,
    string Nome,
    bool Ativa
);