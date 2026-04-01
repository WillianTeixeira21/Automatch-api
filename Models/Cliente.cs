using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoMatch.API.Models;

public class Cliente
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(100)] public string Nome      { get; set; } = "";
    [MaxLength(150)]           public string Email     { get; set; } = "";
    [Required, MaxLength(20)]  public string Telefone  { get; set; } = "";
    [MaxLength(14)]            public string Cpf       { get; set; } = "";
    [MaxLength(100)]           public string Interesse { get; set; } = "";
    [Column(TypeName="decimal(12,2)")] public decimal Orcamento { get; set; }
    [MaxLength(30)]            public string Origem    { get; set; } = "app";
    public int LojaId { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public Loja? Loja { get; set; }
    public ICollection<Visita> Visitas { get; set; } = new List<Visita>();
    public ICollection<Compra> Compras { get; set; } = new List<Compra>();
}
