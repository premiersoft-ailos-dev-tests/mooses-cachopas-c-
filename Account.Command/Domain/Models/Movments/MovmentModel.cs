using Bankmore.Accounts.Command.Domain.Enums.Movments;

namespace Bankmore.Accounts.Command.Domain.Models.Movments;

public class MovmentModel
{
    public int IdConta {get; set;}
    public decimal Valor {get; set;}
    public MovmentType MovmentType {get; set;}
    public DateTime Data {get; set;}
    
    public MovmentModel() {}
}