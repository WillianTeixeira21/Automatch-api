using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMatch.API.Data;
using AutoMatch.API.DTOs;
using AutoMatch.API.Models;
using System.Security.Cryptography;
using System.Text;

namespace AutoMatch.API.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    public AuthController(AppDbContext db) => _db = db;

    [HttpPost("cliente/cadastro")]
    public async Task<ActionResult> CadastroCliente([FromBody] CadastroClienteDto dto)
    {
        if (await _db.Usuarios.AnyAsync(u => u.Email == dto.Email && u.Tipo == "cliente"))
            return BadRequest(new { message = "E-mail já cadastrado." });

        var usuario = new Usuario
        {
            Nome      = dto.Nome,
            Email     = dto.Email,
            Telefone  = dto.Telefone,
            Cpf       = dto.Cpf ?? "",
            SenhaHash = HashSenha(dto.Senha),
            Tipo      = "cliente",
            CriadoEm  = DateTime.UtcNow
        };
        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();

        return Ok(new { id = usuario.Id, nome = usuario.Nome, email = usuario.Email, token = GerarToken(usuario.Id, "cliente") });
    }

    [HttpPost("cliente/login")]
    public async Task<ActionResult> LoginCliente([FromBody] LoginDto dto)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Email == dto.Email && u.Tipo == "cliente");
        if (usuario is null || usuario.SenhaHash != HashSenha(dto.Senha))
            return Unauthorized(new { message = "E-mail ou senha incorretos." });

        return Ok(new { id = usuario.Id, nome = usuario.Nome, email = usuario.Email, token = GerarToken(usuario.Id, "cliente") });
    }

    [HttpPost("lojista/cadastro")]
    public async Task<ActionResult> CadastroLojista([FromBody] CadastroLojistaDto dto)
    {
        if (await _db.Usuarios.AnyAsync(u => u.Email == dto.Email && u.Tipo == "lojista"))
            return BadRequest(new { message = "E-mail já cadastrado." });

        var loja = new Loja
        {
            Nome     = dto.LojaNome,
            Cnpj     = dto.Cnpj ?? "",
            Telefone = dto.Telefone,
            Whatsapp = dto.Telefone,
            Email    = dto.Email,
            Cidade   = dto.Cidade ?? "",
            Estado   = dto.Estado ?? "",
            Plano    = dto.Plano
        };
        _db.Lojas.Add(loja);
        await _db.SaveChangesAsync();

        var usuario = new Usuario
        {
            Nome      = dto.Nome,
            Email     = dto.Email,
            Telefone  = dto.Telefone,
            SenhaHash = HashSenha(dto.Senha),
            Tipo      = "lojista",
            LojaId    = loja.Id,
            CriadoEm  = DateTime.UtcNow
        };
        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();

        return Ok(new { id = usuario.Id, nome = usuario.Nome, email = usuario.Email, lojaId = loja.Id, lojaNome = loja.Nome, plano = loja.Plano, token = GerarToken(usuario.Id, "lojista") });
    }

    [HttpPost("lojista/login")]
    public async Task<ActionResult> LoginLojista([FromBody] LoginDto dto)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Email == dto.Email && u.Tipo == "lojista");
        if (usuario is null || usuario.SenhaHash != HashSenha(dto.Senha))
            return Unauthorized(new { message = "E-mail ou senha incorretos." });

        var loja = await _db.Lojas.FindAsync(usuario.LojaId);
        return Ok(new { id = usuario.Id, nome = usuario.Nome, email = usuario.Email, lojaId = loja?.Id ?? 1, lojaNome = loja?.Nome ?? "", plano = loja?.Plano ?? "basico", token = GerarToken(usuario.Id, "lojista") });
    }

    private static string HashSenha(string senha)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(senha + "automatch_salt_2024"));
        return Convert.ToBase64String(bytes);
    }

    private static string GerarToken(int userId, string tipo)
    {
        var raw   = $"{userId}:{tipo}:{DateTime.UtcNow.Ticks}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return $"{userId}_{tipo}_{Convert.ToBase64String(bytes)[..16]}";
    }
}