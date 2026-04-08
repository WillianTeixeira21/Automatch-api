using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMatch.API.Data;
using AutoMatch.API.DTOs;
using AutoMatch.API.Models;

namespace AutoMatch.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class VeiculosController : ControllerBase
{
    private readonly AppDbContext _db;
    public VeiculosController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VeiculoDto>>> GetVeiculos([FromQuery] VeiculoFiltroDto filtro)
    {
        var query = _db.Veiculos.AsQueryable();

        if (filtro.LojaId.HasValue)  query = query.Where(v => v.LojaId == filtro.LojaId);
        if (!string.IsNullOrWhiteSpace(filtro.Tipo)) query = query.Where(v => v.Tipo == filtro.Tipo);
        if (filtro.PrecoMax.HasValue) query = query.Where(v => v.Preco <= filtro.PrecoMax);
        if (filtro.Destaque == true)  query = query.Where(v => v.Destaque);

        // Filtro por cidade/estado (join com Lojas)
        if (!string.IsNullOrWhiteSpace(filtro.Cidade))
        {
            var cidadeLower = filtro.Cidade.ToLower();
            query = query.Where(v => _db.Lojas.Any(l => l.Id == v.LojaId && l.Cidade.ToLower().Contains(cidadeLower)));
        }
        if (!string.IsNullOrWhiteSpace(filtro.Estado))
        {
            var estadoLower = filtro.Estado.ToLower();
            query = query.Where(v => _db.Lojas.Any(l => l.Id == v.LojaId && l.Estado.ToLower() == estadoLower));
        }

        if (!string.IsNullOrWhiteSpace(filtro.Busca))
        {
            var b = filtro.Busca.ToLower();
            query = query.Where(v => v.Marca.ToLower().Contains(b) ||
                                     v.Modelo.ToLower().Contains(b) ||
                                     v.Versao.ToLower().Contains(b));
        }

        var total = await query.CountAsync();

        // Projection com dados da loja (evita Include + circular reference)
        var veiculos = await query
            .OrderByDescending(v => v.Destaque).ThenBy(v => v.Preco)
            .Skip((filtro.Page - 1) * filtro.PerPage).Take(filtro.PerPage)
            .Select(v => new VeiculoDto
            {
                Id          = v.Id,
                Marca       = v.Marca,
                Modelo      = v.Modelo,
                Versao      = v.Versao,
                Ano         = v.Ano,
                Preco       = v.Preco,
                Km          = v.Km,
                Cor         = v.Cor,
                Tipo        = v.Tipo,
                Cambio      = v.Cambio,
                Combustivel = v.Combustivel,
                Potencia    = v.Potencia,
                Portas      = v.Portas,
                FotoUrl     = v.FotoUrl,
                Opcionais   = v.Opcionais == null ? Array.Empty<string>()
                    : v.Opcionais.Split(',', StringSplitOptions.RemoveEmptyEntries),
                Fotos       = v.Fotos == null || v.Fotos == "" ? Array.Empty<string>()
                    : v.Fotos.Split(',', StringSplitOptions.RemoveEmptyEntries),
                Destaque    = v.Destaque,
                LojaNome    = _db.Lojas.Where(l => l.Id == v.LojaId).Select(l => l.Nome).FirstOrDefault() ?? "",
                Cidade      = _db.Lojas.Where(l => l.Id == v.LojaId).Select(l => l.Cidade).FirstOrDefault() ?? "",
                Estado      = _db.Lojas.Where(l => l.Id == v.LojaId).Select(l => l.Estado).FirstOrDefault() ?? ""
            })
            .ToListAsync();

        Response.Headers.Append("X-Total-Count", total.ToString());
        return Ok(veiculos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VeiculoDto>> GetVeiculo(int id)
    {
        var v = await _db.Veiculos.FindAsync(id);
        if (v is null) return NotFound(new { message = "Veículo não encontrado." });

        var loja = await _db.Lojas.FindAsync(v.LojaId);
        return Ok(ToDto(v, loja));
    }

    [HttpPost]
    public async Task<ActionResult<VeiculoDto>> CriarVeiculo([FromBody] Veiculo veiculo)
    {
        _db.Veiculos.Add(veiculo);
        await _db.SaveChangesAsync();
        var loja = await _db.Lojas.FindAsync(veiculo.LojaId);
        return CreatedAtAction(nameof(GetVeiculo), new { id = veiculo.Id }, ToDto(veiculo, loja));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> AtualizarVeiculo(int id, [FromBody] Veiculo veiculo)
    {
        if (id != veiculo.Id) return BadRequest();
        _db.Entry(veiculo).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletarVeiculo(int id)
    {
        var v = await _db.Veiculos.FindAsync(id);
        if (v is null) return NotFound();
        _db.Veiculos.Remove(v);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Lista cidades distintas que possuem veículos cadastrados.
    /// </summary>
    [HttpGet("cidades")]
    public async Task<ActionResult<IEnumerable<string>>> GetCidades()
    {
        var cidades = await _db.Veiculos
            .Join(_db.Lojas, v => v.LojaId, l => l.Id, (v, l) => l.Cidade)
            .Where(c => c != null && c != "")
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
        return Ok(cidades);
    }

    private static VeiculoDto ToDto(Veiculo v, Loja? loja) => new()
    {
        Id = v.Id, Marca = v.Marca, Modelo = v.Modelo, Versao = v.Versao,
        Ano = v.Ano, Preco = v.Preco, Km = v.Km, Cor = v.Cor,
        Tipo = v.Tipo, Cambio = v.Cambio, Combustivel = v.Combustivel,
        Potencia = v.Potencia, Portas = v.Portas, FotoUrl = v.FotoUrl,
        Opcionais = string.IsNullOrEmpty(v.Opcionais) ? Array.Empty<string>()
            : v.Opcionais.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(o => o.Trim()).ToArray(),
        Fotos = string.IsNullOrEmpty(v.Fotos) ? Array.Empty<string>()
            : v.Fotos.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(f => f.Trim()).ToArray(),
        Destaque = v.Destaque,
        LojaNome = loja?.Nome ?? "",
        Cidade   = loja?.Cidade ?? "",
        Estado   = loja?.Estado ?? ""
    };
}
