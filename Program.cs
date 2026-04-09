using Microsoft.EntityFrameworkCore;
using AutoMatch.API.Data;
using AutoMatch.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "AutoMatch API", Version = "v1" });
});

// ── Cloudinary ──
builder.Services.AddSingleton<CloudinaryService>();

// ── Database (Azure SQL Server) ──
var connectionString = Environment.GetEnvironmentVariable("AZURE_SQL_CONNECTION")
    ?? "Server=tcp:sql-primeavenue-dev.database.windows.net,1433;Initial Catalog=AutoMatchDB;Persist Security Info=False;User ID=sqladmin;Password=zudcez-3cecpy-jossoH;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(connectionString));

builder.Services.AddCors(opt =>
    opt.AddPolicy("AutoMatchPolicy", p =>
        p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Services.AddControllers().AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.ReferenceHandler =
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    opt.JsonSerializerOptions.PropertyNamingPolicy =
        System.Text.Json.JsonNamingPolicy.CamelCase;
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AutoMatchPolicy");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    // Criação das tabelas (T-SQL para Azure SQL Server)
    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Usuarios')
        CREATE TABLE [Usuarios] (
            [Id] INT IDENTITY(1,1) PRIMARY KEY,
            [Nome] NVARCHAR(100) NOT NULL,
            [Email] NVARCHAR(150) NOT NULL,
            [Telefone] NVARCHAR(20) NULL,
            [Cpf] NVARCHAR(14) NULL,
            [SenhaHash] NVARCHAR(256) NOT NULL,
            [Tipo] NVARCHAR(10) NOT NULL DEFAULT 'cliente',
            [LojaId] INT NULL,
            [CriadoEm] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
        );

        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Lojas')
        CREATE TABLE [Lojas] (
            [Id] INT IDENTITY(1,1) PRIMARY KEY,
            [Nome] NVARCHAR(100) NOT NULL,
            [Cnpj] NVARCHAR(18) NULL,
            [Telefone] NVARCHAR(20) NULL,
            [Whatsapp] NVARCHAR(20) NULL,
            [Email] NVARCHAR(150) NULL,
            [Endereco] NVARCHAR(200) NULL,
            [Cidade] NVARCHAR(80) NULL,
            [Estado] NVARCHAR(2) NULL,
            [LogoUrl] NVARCHAR(500) NULL,
            [Plano] NVARCHAR(20) NULL,
            [CriadoEm] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
        );

        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Veiculos')
        CREATE TABLE [Veiculos] (
            [Id] INT IDENTITY(1,1) PRIMARY KEY,
            [Marca] NVARCHAR(50) NOT NULL,
            [Modelo] NVARCHAR(80) NOT NULL,
            [Versao] NVARCHAR(100) NULL,
            [Ano] INT NULL,
            [Preco] DECIMAL(12,2) NULL,
            [Km] INT NULL,
            [Cor] NVARCHAR(40) NULL,
            [Tipo] NVARCHAR(20) NULL,
            [Cambio] NVARCHAR(30) NULL,
            [Combustivel] NVARCHAR(30) NULL,
            [Potencia] NVARCHAR(20) NULL,
            [Portas] INT NULL,
            [FotoUrl] NVARCHAR(500) NULL,
            [Opcionais] NVARCHAR(500) NULL,
            [Fotos] NVARCHAR(MAX) NULL DEFAULT '',
            [Destaque] BIT DEFAULT 0,
            [LojaId] INT DEFAULT 1,
            [CriadoEm] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
        );

        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Clientes')
        CREATE TABLE [Clientes] (
            [Id] INT IDENTITY(1,1) PRIMARY KEY,
            [Nome] NVARCHAR(100) NOT NULL,
            [Email] NVARCHAR(150) NULL,
            [Telefone] NVARCHAR(20) NOT NULL,
            [Cpf] NVARCHAR(14) NULL,
            [Interesse] NVARCHAR(100) NULL,
            [Orcamento] DECIMAL(12,2) NULL,
            [Origem] NVARCHAR(30) NULL,
            [LojaId] INT DEFAULT 1,
            [CriadoEm] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
        );

        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Captacoes')
        CREATE TABLE [Captacoes] (
            [Id] INT IDENTITY(1,1) PRIMARY KEY,
            [Nome] NVARCHAR(100) NOT NULL,
            [Telefone] NVARCHAR(20) NOT NULL,
            [Email] NVARCHAR(150) NULL,
            [Interesse] NVARCHAR(30) NULL,
            [Orcamento] NVARCHAR(30) NULL,
            [LojaId] INT DEFAULT 1,
            [CriadoEm] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
        );

        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Visitas')
        CREATE TABLE [Visitas] (
            [Id] INT IDENTITY(1,1) PRIMARY KEY,
            [ClienteId] INT NOT NULL,
            [VeiculoId] INT NOT NULL,
            [Timestamp] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
        );

        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Compras')
        CREATE TABLE [Compras] (
            [Id] INT IDENTITY(1,1) PRIMARY KEY,
            [ClienteId] INT NOT NULL,
            [VeiculoId] INT NOT NULL,
            [EtapaAtual] INT DEFAULT 1,
            [DataCompra] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
        );
    ");

    // Seed da loja padrão
    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT 1 FROM [Lojas] WHERE [Id] = 1)
        BEGIN
            SET IDENTITY_INSERT [Lojas] ON;
            INSERT INTO [Lojas] ([Id], [Nome], [Cnpj], [Telefone], [Whatsapp], [Email], [Endereco], [Cidade], [Estado], [Plano])
            VALUES (1, 'AutoMatch Demo', '12.345.678/0001-90', '(11) 3722-4545', '5511983970098', 'contato@automatch.com.br', 'Av. Paulista, 1000', N'São Paulo', 'SP', 'enterprise');
            SET IDENTITY_INSERT [Lojas] OFF;
        END
    ");

    // Adicionar coluna Fotos se não existir (para bancos já criados)
    try {
        db.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Veiculos') AND name = 'Fotos')
            ALTER TABLE [Veiculos] ADD [Fotos] NVARCHAR(MAX) NULL DEFAULT '';
        ");
    } catch { /* coluna já existe */ }
}

app.Run();
