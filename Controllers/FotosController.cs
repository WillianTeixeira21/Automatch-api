using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMatch.API.Data;
using AutoMatch.API.Services;

namespace AutoMatch.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FotosController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly CloudinaryService _cloudinary;

    public FotosController(AppDbContext db, CloudinaryService cloudinary)
    {
        _db = db;
        _cloudinary = cloudinary;
    }

    /// <summary>
    /// Upload de uma foto para um veículo.
    /// POST /api/fotos/upload
    /// Form-data: file (imagem), veiculoId (int)
    /// Retorna: { url: "https://..." }
    /// </summary>
    [HttpPost("upload")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    [Consumes("multipart/form-data")] // 10MB max
    public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromForm] int veiculoId)
    {
        // Validações
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Nenhum arquivo enviado." });

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
            return BadRequest(new { message = "Formato inválido. Use JPG, PNG ou WebP." });

        if (file.Length > 10 * 1024 * 1024)
            return BadRequest(new { message = "Arquivo muito grande. Máximo 10MB." });

        var veiculo = await _db.Veiculos.FindAsync(veiculoId);
        if (veiculo == null)
            return NotFound(new { message = "Veículo não encontrado." });

        // Verificar limite de fotos (15)
        var fotosAtuais = string.IsNullOrEmpty(veiculo.Fotos)
            ? Array.Empty<string>()
            : veiculo.Fotos.Split(',', StringSplitOptions.RemoveEmptyEntries);

        if (fotosAtuais.Length >= 15)
            return BadRequest(new { message = "Limite de 15 fotos atingido." });

        // Upload para o Cloudinary
        using var stream = file.OpenReadStream();
        var url = await _cloudinary.UploadImageAsync(stream, file.FileName);

        if (url == null)
            return StatusCode(500, new { message = "Erro ao fazer upload da imagem." });

        // Atualizar o veículo no banco
        var novasFotos = fotosAtuais.Append(url).ToArray();
        veiculo.Fotos = string.Join(",", novasFotos);

        // A primeira foto também vira a FotoUrl (capa)
        if (string.IsNullOrEmpty(veiculo.FotoUrl))
            veiculo.FotoUrl = url;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            url,
            totalFotos = novasFotos.Length,
            fotos = novasFotos
        });
    }

    /// <summary>
    /// Define qual foto é a capa (FotoUrl principal).
    /// PUT /api/fotos/capa
    /// Body: { veiculoId: 1, fotoUrl: "https://..." }
    /// </summary>
    [HttpPut("capa")]
    public async Task<IActionResult> DefinirCapa([FromBody] CapaRequest req)
    {
        var veiculo = await _db.Veiculos.FindAsync(req.VeiculoId);
        if (veiculo == null)
            return NotFound(new { message = "Veículo não encontrado." });

        veiculo.FotoUrl = req.FotoUrl;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Capa atualizada.", fotoUrl = req.FotoUrl });
    }

    /// <summary>
    /// Remove uma foto do veículo.
    /// DELETE /api/fotos?veiculoId=1&fotoUrl=https://...
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> RemoverFoto([FromQuery] int veiculoId, [FromQuery] string fotoUrl)
    {
        var veiculo = await _db.Veiculos.FindAsync(veiculoId);
        if (veiculo == null)
            return NotFound(new { message = "Veículo não encontrado." });

        var fotos = string.IsNullOrEmpty(veiculo.Fotos)
            ? Array.Empty<string>()
            : veiculo.Fotos.Split(',', StringSplitOptions.RemoveEmptyEntries);

        var novasFotos = fotos.Where(f => f != fotoUrl).ToArray();
        veiculo.Fotos = string.Join(",", novasFotos);

        // Se removeu a capa, promove a próxima
        if (veiculo.FotoUrl == fotoUrl)
            veiculo.FotoUrl = novasFotos.FirstOrDefault() ?? "";

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Foto removida.",
            totalFotos = novasFotos.Length,
            fotos = novasFotos,
            fotoUrl = veiculo.FotoUrl
        });
    }

    /// <summary>
    /// Lista todas as fotos de um veículo.
    /// GET /api/fotos/{veiculoId}
    /// </summary>
    [HttpGet("{veiculoId:int}")]
    public async Task<IActionResult> ListarFotos(int veiculoId)
    {
        var veiculo = await _db.Veiculos.FindAsync(veiculoId);
        if (veiculo == null)
            return NotFound(new { message = "Veículo não encontrado." });

        var fotos = string.IsNullOrEmpty(veiculo.Fotos)
            ? Array.Empty<string>()
            : veiculo.Fotos.Split(',', StringSplitOptions.RemoveEmptyEntries);

        return Ok(new
        {
            veiculoId,
            fotoUrl = veiculo.FotoUrl,
            totalFotos = fotos.Length,
            fotos
        });
    }
}

public class CapaRequest
{
    public int VeiculoId { get; set; }
    public string FotoUrl { get; set; } = "";
}
