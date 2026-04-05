using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoMatch.API.Models;

public class Veiculo
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(50)]  public string Marca       { get; set; } = "";
    [Required, MaxLength(80)]  public string Modelo      { get; set; } = "";
    [MaxLength(100)]           public string Versao      { get; set; } = "";
    public int Ano { get; set; }
    [Column(TypeName="decimal(12,2)")] public decimal Preco { get; set; }
    public int Km { get; set; }
    [MaxLength(40)]  public string Cor         { get; set; } = "";
    [MaxLength(20)]  public string Tipo        { get; set; } = "";
    [MaxLength(30)]  public string Cambio      { get; set; } = "";
    [MaxLength(30)]  public string Combustivel { get; set; } = "";
    [MaxLength(20)]  public string Potencia    { get; set; } = "";
    public int Portas { get; set; }
    [MaxLength(500)] public string FotoUrl     { get; set; } = "";
    [MaxLength(500)] public string Opcionais   { get; set; } = "";
    
    /// <summary>
    /// URLs das fotos separadas por vírgula (mesma lógica de Opcionais).
    /// FotoUrl continua como foto principal (capa) para compatibilidade.
    /// </summary>
    [MaxLength(5000)] public string Fotos { get; set; } = "";
    
    public bool Destaque { get; set; } = false;
    public int LojaId { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public Loja? Loja { get; set; }
    public ICollection<Visita> Visitas { get; set; } = new List<Visita>();
}
