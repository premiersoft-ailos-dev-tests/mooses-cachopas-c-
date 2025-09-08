using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infra.Db.Entities;

public sealed class TransfersDbModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdTransferencia { get; set; }
    public int IdContaCorrenteOrigem { get; set; } = default!;
    public int IdContaCorrenteDestino { get; set; } = default!;
    public DateTime DataMovimento { get; set; } = default!;
    public decimal Valor { get; set; }
}