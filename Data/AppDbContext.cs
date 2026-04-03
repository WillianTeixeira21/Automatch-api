using Microsoft.EntityFrameworkCore;
using AutoMatch.API.Models;

namespace AutoMatch.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Loja>     Lojas     { get; set; }
    public DbSet<Usuario>  Usuarios  { get; set; }
    public DbSet<Veiculo>  Veiculos  { get; set; }
    public DbSet<Cliente>  Clientes  { get; set; }
    public DbSet<Visita>   Visitas   { get; set; }
    public DbSet<Compra>   Compras   { get; set; }
    public DbSet<Captacao> Captacoes { get; set; }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Veiculo>()
          .HasOne(v => v.Loja).WithMany(l => l.Veiculos)
          .HasForeignKey(v => v.LojaId).OnDelete(DeleteBehavior.Restrict);

        mb.Entity<Cliente>()
          .HasOne(c => c.Loja).WithMany(l => l.Clientes)
          .HasForeignKey(c => c.LojaId).OnDelete(DeleteBehavior.Restrict);

        mb.Entity<Visita>()
          .HasOne(v => v.Cliente).WithMany(c => c.Visitas)
          .HasForeignKey(v => v.ClienteId).OnDelete(DeleteBehavior.Cascade);

        mb.Entity<Visita>()
          .HasOne(v => v.Veiculo).WithMany(ve => ve.Visitas)
          .HasForeignKey(v => v.VeiculoId).OnDelete(DeleteBehavior.Restrict);

        mb.Entity<Compra>()
          .HasOne(c => c.Cliente).WithMany(cl => cl.Compras)
          .HasForeignKey(c => c.ClienteId).OnDelete(DeleteBehavior.Cascade);

        mb.Entity<Veiculo>().HasIndex(v => v.Tipo);
        mb.Entity<Veiculo>().HasIndex(v => v.LojaId);
        mb.Entity<Visita>().HasIndex(v => v.ClienteId);
        mb.Entity<Cliente>().HasIndex(c => c.LojaId);

        mb.Entity<Loja>().HasData(new Loja
        {
            Id = 1, Nome = "13Car Multimarcas",
            Cnpj = "12.345.678/0001-90",
            Telefone = "(11) 3722-4545",
            Whatsapp = "5511983970098",
            Email = "13car@13car.com.br",
            Endereco = "Av. Eliseu de Almeida, 432 - Butantã",
            Cidade = "São Paulo", Estado = "SP", Plano = "enterprise"
        });
    }
}
