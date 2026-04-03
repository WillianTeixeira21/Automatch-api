using System.ComponentModel.DataAnnotations;

namespace AutoMatch.API.Models;

public class Usuario
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Nome { get; set; } = "";

    [Required, MaxLength(150)]
    public string Email { get; set; } = "";

    [MaxLength(20)]
    public string Telefone { get; set; } = "";

    [MaxLength(14)]
    public string Cpf { get; set; } = "";

    [Required, MaxLength(256)]
    public string SenhaHash { get; set; } = "";

    [Required, MaxLength(10)]
    public string Tipo { get; set; } = "cliente";

    public int? LojaId { get; set; }

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public Loja? Loja { get; set; }
}