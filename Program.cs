using Microsoft.EntityFrameworkCore;
using AutoMatch.API.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "AutoMatch API", Version = "v1" });
});

// Railway injeta DATABASE_URL automaticamente
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
string connectionString;

if (!string.IsNullOrEmpty(databaseUrl))
{
    var uri       = new Uri(databaseUrl);
    var userInfo  = uri.UserInfo.Split(':');
    connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true;";
    builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(connectionString));
}
else
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
    builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(connectionString));
}

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
    
    // Força criação das tabelas se não existirem
    var sql = db.Database.GetConnectionString();
    db.Database.ExecuteSqlRaw(@"
        CREATE TABLE IF NOT EXISTS ""Usuarios"" (
            ""Id"" SERIAL PRIMARY KEY,
            ""Nome"" VARCHAR(100) NOT NULL,
            ""Email"" VARCHAR(150) NOT NULL,
            ""Telefone"" VARCHAR(20),
            ""Cpf"" VARCHAR(14),
            ""SenhaHash"" VARCHAR(256) NOT NULL,
            ""Tipo"" VARCHAR(10) NOT NULL DEFAULT 'cliente',
            ""LojaId"" INT,
            ""CriadoEm"" TIMESTAMP NOT NULL DEFAULT NOW()
        );
        CREATE TABLE IF NOT EXISTS ""Lojas"" (
            ""Id"" SERIAL PRIMARY KEY,
            ""Nome"" VARCHAR(100) NOT NULL,
            ""Cnpj"" VARCHAR(18),
            ""Telefone"" VARCHAR(20),
            ""Whatsapp"" VARCHAR(20),
            ""Email"" VARCHAR(150),
            ""Endereco"" VARCHAR(200),
            ""Cidade"" VARCHAR(80),
            ""Estado"" VARCHAR(2),
            ""LogoUrl"" VARCHAR(500),
            ""Plano"" VARCHAR(20),
            ""CriadoEm"" TIMESTAMP NOT NULL DEFAULT NOW()
        );
        CREATE TABLE IF NOT EXISTS ""Veiculos"" (
            ""Id"" SERIAL PRIMARY KEY,
            ""Marca"" VARCHAR(50) NOT NULL,
            ""Modelo"" VARCHAR(80) NOT NULL,
            ""Versao"" VARCHAR(100),
            ""Ano"" INT,
            ""Preco"" DECIMAL(12,2),
            ""Km"" INT,
            ""Cor"" VARCHAR(40),
            ""Tipo"" VARCHAR(20),
            ""Cambio"" VARCHAR(30),
            ""Combustivel"" VARCHAR(30),
            ""Potencia"" VARCHAR(20),
            ""Portas"" INT,
            ""FotoUrl"" VARCHAR(500),
            ""Opcionais"" VARCHAR(500),
            ""Destaque"" BOOLEAN DEFAULT FALSE,
            ""LojaId"" INT DEFAULT 1,
            ""CriadoEm"" TIMESTAMP NOT NULL DEFAULT NOW()
        );
        CREATE TABLE IF NOT EXISTS ""Clientes"" (
            ""Id"" SERIAL PRIMARY KEY,
            ""Nome"" VARCHAR(100) NOT NULL,
            ""Email"" VARCHAR(150),
            ""Telefone"" VARCHAR(20) NOT NULL,
            ""Cpf"" VARCHAR(14),
            ""Interesse"" VARCHAR(100),
            ""Orcamento"" DECIMAL(12,2),
            ""Origem"" VARCHAR(30),
            ""LojaId"" INT DEFAULT 1,
            ""CriadoEm"" TIMESTAMP NOT NULL DEFAULT NOW()
        );
        CREATE TABLE IF NOT EXISTS ""Captacoes"" (
            ""Id"" SERIAL PRIMARY KEY,
            ""Nome"" VARCHAR(100) NOT NULL,
            ""Telefone"" VARCHAR(20) NOT NULL,
            ""Email"" VARCHAR(150),
            ""Interesse"" VARCHAR(30),
            ""Orcamento"" VARCHAR(30),
            ""LojaId"" INT DEFAULT 1,
            ""CriadoEm"" TIMESTAMP NOT NULL DEFAULT NOW()
        );
        CREATE TABLE IF NOT EXISTS ""Visitas"" (
            ""Id"" SERIAL PRIMARY KEY,
            ""ClienteId"" INT NOT NULL,
            ""VeiculoId"" INT NOT NULL,
            ""Timestamp"" TIMESTAMP NOT NULL DEFAULT NOW()
        );
        CREATE TABLE IF NOT EXISTS ""Compras"" (
            ""Id"" SERIAL PRIMARY KEY,
            ""ClienteId"" INT NOT NULL,
            ""VeiculoId"" INT NOT NULL,
            ""EtapaAtual"" INT DEFAULT 1,
            ""DataCompra"" TIMESTAMP NOT NULL DEFAULT NOW()
        );
        INSERT INTO ""Lojas"" (""Id"", ""Nome"", ""Cnpj"", ""Telefone"", ""Whatsapp"", ""Email"", ""Endereco"", ""Cidade"", ""Estado"", ""Plano"")
        VALUES (1, '13Car Multimarcas', '12.345.678/0001-90', '(11) 3722-4545', '5511983970098', '13car@13car.com.br', 'Av. Eliseu de Almeida, 432 - Butantã', 'São Paulo', 'SP', 'enterprise')
        ON CONFLICT (""Id"") DO NOTHING;
    ");
}

app.Run();
