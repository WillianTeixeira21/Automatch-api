using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoMatch.API.Models;

public class Loja
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(100)] public string Nome     { get; set; } = "";
    [MaxLength(18)]  public string Cnpj     { get; set; } = "";
    [MaxLength(20)]  public string Telefone { get; set; } = "";
    [MaxLength(20)]  public string Whatsapp { get; set; } = "";
    [MaxLength(150)] public string Email    { get; set; } = "";
    [MaxLength(200)] public string Endereco { get; set; } = "";
    [MaxLength(80)]  public string Cidade   { get; set; } = "";
    [MaxLength(2)]   public string Estado   { get; set; } = "";
    [MaxLength(500)] public string LogoUrl  { get; set; } = "";
    [MaxLength(20)]  public string Plano    { get; set; } = "basico";
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public ICollection<Veiculo> Veiculos { get; set; } = new List<Veiculo>();
    public ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();
}

public class Visita
{
    [Key] public int Id { get; set; }
    public int ClienteId { get; set; }
    public int VeiculoId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Cliente? Cliente { get; set; }
    public Veiculo? Veiculo { get; set; }
}

public class Compra
{
    [Key] public int Id { get; set; }
    public int ClienteId  { get; set; }
    public int VeiculoId  { get; set; }
    public int EtapaAtual { get; set; } = 1;
    public DateTime DataCompra { get; set; } = DateTime.UtcNow;
    public Cliente? Cliente { get; set; }
    public Veiculo? Veiculo { get; set; }
}

public class Captacao
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(100)] public string Nome      { get; set; } = "";
    [Required, MaxLength(20)]  public string Telefone  { get; set; } = "";
    [MaxLength(150)] public string Email     { get; set; } = "";
    [MaxLength(30)]  public string Interesse { get; set; } = "";
    [MaxLength(30)]  public string Orcamento { get; set; } = "";
    public int LojaId { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
