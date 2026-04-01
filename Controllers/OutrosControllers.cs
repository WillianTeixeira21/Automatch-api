using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMatch.API.Data;
using AutoMatch.API.DTOs;
using AutoMatch.API.Models;

namespace AutoMatch.API.Controllers;

// ── Financiamento ─────────────────────────────────────────────────────────────
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FinanciamentoController : ControllerBase
{
    private readonly AppDbContext _db;
    public FinanciamentoController(AppDbContext db) => _db = db;

    [HttpPost("simular")]
    public async Task<ActionResult<IEnumerable<PropostaFinanciamentoDto>>> Simular(
        [FromBody] SimulacaoRequestDto dto)
    {
        var veiculo = await _db.Veiculos.FindAsync(dto.VeiculoId);
        var valorBase = veiculo?.Preco ?? (decimal)dto.ValorEntrada * 2;
        var saldo = (double)(valorBase - dto.ValorEntrada);

        var bancos = new[]
        {
            ("Banco 13Car",    1.19, "pre-aprovado"),
            ("Santander Auto", 1.45, "pre-aprovado"),
            ("Bradesco Auto",  1.65, "em-analise")
        };

        var propostas = bancos.Select((b, i) =>
        {
            var (nome, taxa, status) = b;
            var tm      = taxa / 100.0;
            var n       = (double)dto.PrazoMeses;
            var parcela = saldo * (tm * Math.Pow(1 + tm, n)) / (Math.Pow(1 + tm, n) - 1);
            var total   = parcela * n + (double)dto.ValorEntrada;
            return new PropostaFinanciamentoDto
            {
                Banco = nome, TaxaMensal = taxa, Prazo = dto.PrazoMeses,
                ValorParcela = Math.Round((decimal)parcela, 2),
                TotalPagar   = Math.Round((decimal)total, 2),
                Status = status, MelhorOferta = i == 0
            };
        });
        return Ok(propostas);
    }
}

// ── Loja ──────────────────────────────────────────────────────────────────────
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LojaController : ControllerBase
{
    private readonly AppDbContext _db;
    public LojaController(AppDbContext db) => _db = db;

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Loja>> GetLoja(int id)
    {
        var loja = await _db.Lojas.FindAsync(id);
        if (loja is null) return NotFound();
        return Ok(loja);
    }

    [HttpPost]
    public async Task<ActionResult<Loja>> CriarLoja([FromBody] LojaUpdateDto dto)
    {
        var loja = new Loja
        {
            Nome = dto.Nome, Cnpj = dto.Cnpj, Telefone = dto.Telefone,
            Whatsapp = dto.Whatsapp, Email = dto.Email, Endereco = dto.Endereco,
            Cidade = dto.Cidade, Estado = dto.Estado, LogoUrl = dto.LogoUrl, Plano = dto.Plano
        };
        _db.Lojas.Add(loja);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetLoja), new { id = loja.Id }, loja);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> AtualizarLoja(int id, [FromBody] LojaUpdateDto dto)
    {
        var loja = await _db.Lojas.FindAsync(id);
        if (loja is null) return NotFound();
        loja.Nome = dto.Nome; loja.Cnpj = dto.Cnpj; loja.Telefone = dto.Telefone;
        loja.Whatsapp = dto.Whatsapp; loja.Email = dto.Email; loja.Endereco = dto.Endereco;
        loja.Cidade = dto.Cidade; loja.Estado = dto.Estado; loja.LogoUrl = dto.LogoUrl; loja.Plano = dto.Plano;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

// ── CRM ───────────────────────────────────────────────────────────────────────
[ApiController]
[Route("api/crm")]
[Produces("application/json")]
public class CrmController : ControllerBase
{
    private readonly AppDbContext _db;
    public CrmController(AppDbContext db) => _db = db;

    [HttpGet("stats")]
    public async Task<ActionResult<CrmStatsDto>> GetStats([FromQuery] int lojaId = 1)
    {
        var agora    = DateTime.UtcNow;
        var limSemana = agora.AddDays(-7);
        var limMes    = agora.AddDays(-30);
        var clientes = await _db.Clientes.Where(c => c.LojaId == lojaId).ToListAsync();
        return Ok(new CrmStatsDto
        {
            Leads     = clientes.Count,
            Quentes   = clientes.Count(c => c.CriadoEm >= limSemana),
            Mornos    = clientes.Count(c => c.CriadoEm < limSemana && c.CriadoEm >= limMes),
            Frios     = clientes.Count(c => c.CriadoEm < limMes),
            Captacoes = await _db.Captacoes.CountAsync(c => c.LojaId == lojaId),
            Veiculos  = await _db.Veiculos.CountAsync(v => v.LojaId == lojaId)
        });
    }

    [HttpGet("/api/captacoes")]
    public async Task<ActionResult<IEnumerable<Captacao>>> GetCaptacoes([FromQuery] int lojaId = 1)
        => Ok(await _db.Captacoes.Where(c => c.LojaId == lojaId).OrderByDescending(c => c.CriadoEm).Take(50).ToListAsync());

    [HttpPost("/api/captacoes")]
    public async Task<IActionResult> CriarCaptacao([FromBody] CaptacaoCreateDto dto)
    {
        var cap = new Captacao
        {
            Nome = dto.Nome, Telefone = dto.Telefone, Email = dto.Email,
            Interesse = dto.Interesse, Orcamento = dto.Orcamento, LojaId = dto.LojaId
        };
        _db.Captacoes.Add(cap);
        await _db.SaveChangesAsync();
        return Created("", new IdResponse { Id = cap.Id });
    }

    [HttpPost("/api/disparos/simular")]
    public async Task<ActionResult> SimularDisparo([FromQuery] int lojaId = 1)
    {
        var limite  = DateTime.UtcNow.AddDays(-7);
        var quentes = await _db.Clientes.CountAsync(c => c.LojaId == lojaId && c.CriadoEm >= limite);
        return Ok(new { mensagensEnviadas = quentes, status = "simulado" });
    }
}

// ── Compras ───────────────────────────────────────────────────────────────────
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ComprasController : ControllerBase
{
    private readonly AppDbContext _db;
    public ComprasController(AppDbContext db) => _db = db;

    [HttpGet("{clienteId:int}")]
    public async Task<ActionResult<Compra>> GetCompra(int clienteId)
    {
        var compra = await _db.Compras
            .Where(c => c.ClienteId == clienteId)
            .OrderByDescending(c => c.DataCompra)
            .FirstOrDefaultAsync();
        if (compra is null) return NotFound();
        return Ok(compra);
    }

    [HttpPut("{id:int}/etapa")]
    public async Task<IActionResult> AvancarEtapa(int id, [FromBody] CompraEtapaDto dto)
    {
        var compra = await _db.Compras.FindAsync(id);
        if (compra is null) return NotFound();
        if (dto.EtapaAtual < 1 || dto.EtapaAtual > 5)
            return BadRequest(new { message = "Etapa deve ser entre 1 e 5." });
        compra.EtapaAtual = dto.EtapaAtual;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
