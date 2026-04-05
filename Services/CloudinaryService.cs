using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace AutoMatch.API.Services;

public class CloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService()
    {
        // Railway: configure via variáveis de ambiente
        var cloudName = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME") ?? "";
        var apiKey    = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY") ?? "";
        var apiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET") ?? "";

        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true;
    }

    /// <summary>
    /// Faz upload de uma imagem para o Cloudinary.
    /// Retorna a URL pública ou null em caso de erro.
    /// </summary>
    public async Task<string?> UploadImageAsync(Stream fileStream, string fileName)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Folder = "automatch/veiculos",
            Transformation = new Transformation()
                .Width(1200).Height(800).Crop("limit")   // redimensiona sem cortar
                .Quality("auto:good")                     // compressão inteligente
                .FetchFormat("auto"),                     // webp quando suportado
            UniqueFilename = true,
            Overwrite = false
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.StatusCode == System.Net.HttpStatusCode.OK)
            return result.SecureUrl.ToString();

        Console.WriteLine($"[Cloudinary] Erro: {result.Error?.Message}");
        return null;
    }

    /// <summary>
    /// Remove uma imagem do Cloudinary pelo publicId.
    /// </summary>
    public async Task<bool> DeleteImageAsync(string publicId)
    {
        var result = await _cloudinary.DestroyAsync(new DeletionParams(publicId));
        return result.Result == "ok";
    }
}
