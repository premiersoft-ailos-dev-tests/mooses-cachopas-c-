using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bankmore.Accounts.Command.Infrastructure.Db.Entities;

public class AccountDbModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Numero { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public bool Ativa { get; set; }
    public string Senha { get; set; } = string.Empty;
    public string Salt  { get; set; } = string.Empty;
}