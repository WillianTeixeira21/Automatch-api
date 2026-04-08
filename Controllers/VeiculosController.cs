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
        var query = _db.Veiculos.Include(v => v.Loja).AsQueryable();

        if (filtro.LojaId.HasValue)  query = query.Where(v => v.LojaId == filtro.LojaId);
        if (!string.IsNullOrWhiteSpace(filtro.Tipo)) query = query.Where(v => v.Tipo == filtro.Tipo);
        if (filtro.PrecoMax.HasValue) query = query.Where(v => v.Preco <= filtro.PrecoMax);
        if (filtro.Destaque == true)  query = query.Where(v => v.Destaque);

        // Filtro por cidade/estado (da loja)
        if (!string.IsNullOrWhiteSpace(filtro.Cidade))
            query = query.Where(v => v.Loja != null && v.Loja.Cidade.ToLower().Contains(filtro.Cidade.ToLower()));
        if (!string.IsNullOrWhiteSpace(filtro.Estado))
            query = query.Where(v => v.Loja != null && v.Loja.Estado.ToLower() == filtro.Estado.ToLower());

        if (!string.IsNullOrWhiteSpace(filtro.Busca))
        {
            var b = filtro.Busca.ToLower();
            query = query.Where(v => v.Marca.ToLower().Contains(b) ||
                                     v.Modelo.ToLower().Contains(b) ||
                                     v.Versao.ToLower().Contains(b));
        }

        var total    = await query.CountAsync();
        var veiculos = await query
            .OrderByDescending(v => v.Destaque).ThenBy(v => v.Preco)
            .Skip((filtro.Page - 1) * filtro.PerPage).Take(filtro.PerPage)
            .ToListAsync();

        Response.Headers.Append("X-Total-Count", total.ToString());
        return Ok(veiculos.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VeiculoDto>> GetVeiculo(int id)
    {
        var v = await _db.Veiculos.Include(v => v.Loja).FirstOrDefaultAsync(v => v.Id == id);
        if (v is null) return NotFound(new { message = "Veículo não encontrado." });
        return Ok(ToDto(v));
    }

    [HttpPost]
    public async Task<ActionResult<VeiculoDto>> CriarVeiculo([FromBody] Veiculo veiculo)
    {
        _db.Veiculos.Add(veiculo);
        await _db.SaveChangesAsync();
        // Reload with Loja
        await _db.Entry(veiculo).Reference(v => v.Loja).LoadAsync();
        return CreatedAtAction(nameof(GetVeiculo), new { id = veiculo.Id }, ToDto(veiculo));
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
    /// GET /api/veiculos/cidades
    /// </summary>
    [HttpGet("cidades")]
    public async Task<ActionResult<IEnumerable<string>>> GetCidades()
    {
        var cidades = await _db.Veiculos
            .Include(v => v.Loja)
            .Where(v => v.Loja != null && v.Loja.Cidade != "")
            .Select(v => v.Loja!.Cidade)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
        return Ok(cidades);
    }

    private static VeiculoDto ToDto(Veiculo v) => new()
    {
        Id = v.Id, Marca = v.Marca, Modelo = v.Modelo, Versao = v.Versao,
        Ano = v.Ano, Preco = v.Preco, Km = v.Km, Cor = v.Cor,
        Tipo = v.Tipo, Cambio = v.Cambio, Combustivel = v.Combustivel,
        Potencia = v.Potencia, Portas = v.Portas, FotoUrl = v.FotoUrl,
        Opcionais = v.Opcionais.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(o => o.Trim()).ToArray(),
        Fotos = string.IsNullOrEmpty(v.Fotos)
            ? Array.Empty<string>()
            : v.Fotos.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(f => f.Trim()).ToArray(),
        Destaque = v.Destaque,
        LojaNome = v.Loja?.Nome ?? "",
        Cidade   = v.Loja?.Cidade ?? "",
        Estado   = v.Loja?.Estado ?? ""
    };
}
