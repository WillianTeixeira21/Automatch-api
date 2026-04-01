using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMatch.API.Data;
using AutoMatch.API.DTOs;
using AutoMatch.API.Models;

namespace AutoMatch.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LeadsController : ControllerBase
{
    private readonly AppDbContext _db;
    public LeadsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LeadDto>>> GetLeads(
        [FromQuery] string? status, [FromQuery] int lojaId = 1,
        [FromQuery] int page = 1,   [FromQuery] int perPage = 50)
    {
        var agora = DateTime.UtcNow;
        var clientes = await _db.Clientes
            .Include(c => c.Visitas).ThenInclude(v => v.Veiculo)
            .Where(c => c.LojaId == lojaId)
            .OrderByDescending(c => c.CriadoEm)
            .Skip((page - 1) * perPage).Take(perPage)
            .ToListAsync();

        var leads = clientes.Select(c => ToLeadDto(c, agora)).ToList();
        if (!string.IsNullOrEmpty(status))
            leads = leads.Where(l => l.Status == status).ToList();
        return Ok(leads);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LeadDto>> GetLead(int id)
    {
        var c = await _db.Clientes
            .Include(c => c.Visitas).ThenInclude(v => v.Veiculo)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (c is null) return NotFound();
        return Ok(ToLeadDto(c, DateTime.UtcNow));
    }

    [HttpGet("{id:int}/recomendacoes")]
    public async Task<ActionResult<IEnumerable<VeiculoDto>>> GetRecomendacoes(int id)
    {
        var cliente = await _db.Clientes.FindAsync(id);
        if (cliente is null) return NotFound();
        var query = _db.Veiculos.AsQueryable();
        if (!string.IsNullOrEmpty(cliente.Interesse))
        {
            var tipo = cliente.Interesse.Split(' ').First();
            query = query.Where(v => v.Tipo == tipo);
        }
        else query = query.Where(v => v.Destaque);
        var recs = await query.Take(6).ToListAsync();
        return Ok(recs.Select(v => new VeiculoDto
        {
            Id = v.Id, Marca = v.Marca, Modelo = v.Modelo,
            Ano = v.Ano, Preco = v.Preco, FotoUrl = v.FotoUrl, Tipo = v.Tipo
        }));
    }

    [HttpPost("/api/clientes")]
    public async Task<ActionResult> CriarCliente([FromBody] ClienteCreateDto dto)
    {
        var cliente = new Cliente
        {
            Nome = dto.Nome, Telefone = dto.Telefone, Email = dto.Email,
            Interesse = dto.PerfilNome, Origem = dto.Origem,
            Orcamento = dto.Orcamento, LojaId = dto.LojaId
        };
        _db.Clientes.Add(cliente);
        await _db.SaveChangesAsync();
        return Created($"/api/leads/{cliente.Id}", new IdResponse { Id = cliente.Id });
    }

    [HttpPost("/api/visitas")]
    public async Task<IActionResult> RegistrarVisita([FromBody] VisitaCreateDto dto)
    {
        _db.Visitas.Add(new Visita { ClienteId = dto.ClienteId, VeiculoId = dto.VeiculoId });
        await _db.SaveChangesAsync();
        return Created("", null);
    }

    private static LeadDto ToLeadDto(Cliente c, DateTime agora)
    {
        var dias   = (agora - c.CriadoEm).TotalDays;
        var status = dias < 7 ? "quente" : dias < 30 ? "morno" : "frio";
        return new LeadDto
        {
            Id = c.Id, Nome = c.Nome, Email = c.Email, Telefone = c.Telefone,
            Interesse = c.Interesse, Orcamento = c.Orcamento, Origem = c.Origem,
            Status = status, UltimaInteracao = c.CriadoEm, TotalVisitas = c.Visitas.Count,
            VeiculosVisitados = c.Visitas.OrderByDescending(v => v.Timestamp).Take(5)
                .Where(v => v.Veiculo != null)
                .Select(v => new VeiculoDto
                {
                    Id = v.Veiculo!.Id, Marca = v.Veiculo.Marca, Modelo = v.Veiculo.Modelo,
                    Ano = v.Veiculo.Ano, Preco = v.Veiculo.Preco, FotoUrl = v.Veiculo.FotoUrl, Tipo = v.Veiculo.Tipo
                }).ToList()
        };
    }
}
