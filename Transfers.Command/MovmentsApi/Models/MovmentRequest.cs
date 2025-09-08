namespace Infra.Models;

public record MovmentRequest
{
    public MovmentRequest() {}

    public MovmentRequest(int pNumero, decimal pValor, string pTipoOperacao)
    {
        numeroConta = pNumero;
        valor = pValor;
        tipoOperacao = pTipoOperacao;
    }
    
    public int numeroConta { get; set; } 
    public decimal valor { get; set; } 
    public string tipoOperacao  { get; set; } = string.Empty;
}