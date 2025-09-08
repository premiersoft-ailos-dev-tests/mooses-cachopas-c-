using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infra.Db.Entities;

public class AccountDbModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Numero { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool Ativa { get; set; }
}