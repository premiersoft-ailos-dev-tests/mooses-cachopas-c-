using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bankmore.Accounts.Command.Infrastructure.Db.Entities;

public class MovmentsDbModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdMovimento { get; set; }
    public int IdContaCorrente { get; set; }
    public DateTime DataMovimento { get; set; }
    public string TipoMovimento { get; set; } = default!;
    public decimal Valor { get; set; }
}