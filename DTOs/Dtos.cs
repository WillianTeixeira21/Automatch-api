namespace AutoMatch.API.DTOs;

public class LoginDto
{
    public string Email { get; set; } = "";
    public string Senha { get; set; } = "";
}

public class CadastroClienteDto
{
    public string  Nome     { get; set; } = "";
    public string  Email    { get; set; } = "";
    public string  Telefone { get; set; } = "";
    public string? Cpf      { get; set; }
    public string  Senha    { get; set; } = "";
}

public class CadastroLojistaDto
{
    public string  Nome     { get; set; } = "";
    public string  Email    { get; set; } = "";
    public string  Telefone { get; set; } = "";
    public string  Senha    { get; set; } = "";
    public string  LojaNome { get; set; } = "";
    public string? Cnpj     { get; set; }
    public string? Cidade   { get; set; }
    public string? Estado   { get; set; }
    public string  Plano    { get; set; } = "basico";
}

public class VeiculoDto
{
    public int      Id          { get; set; }
    public string   Marca       { get; set; } = "";
    public string   Modelo      { get; set; } = "";
    public string   Versao      { get; set; } = "";
    public int      Ano         { get; set; }
    public decimal  Preco       { get; set; }
    public int      Km          { get; set; }
    public string   Cor         { get; set; } = "";
    public string   Tipo        { get; set; } = "";
    public string   Cambio      { get; set; } = "";
    public string   Combustivel { get; set; } = "";
    public string   Potencia    { get; set; } = "";
    public int      Portas      { get; set; }
    public string   FotoUrl     { get; set; } = "";
    public string[] Opcionais   { get; set; } = [];
    public bool     Destaque    { get; set; }
}

public class VeiculoFiltroDto
{
    public string?  Busca    { get; set; }
    public string?  Tipo     { get; set; }
    public decimal? PrecoMax { get; set; }
    public bool?    Destaque { get; set; }
    public int?     LojaId   { get; set; }
    public int      Page     { get; set; } = 1;
    public int      PerPage  { get; set; } = 20;
}

public class ClienteCreateDto
{
    public string  Nome       { get; set; } = "";
    public string  Telefone   { get; set; } = "";
    public string  Email      { get; set; } = "";
    public int     PerfilId   { get; set; }
    public string  PerfilNome { get; set; } = "";
    public string  Origem     { get; set; } = "app";
    public int     LojaId     { get; set; } = 1;
    public decimal Orcamento  { get; set; }
}

public class VisitaCreateDto
{
    public int ClienteId { get; set; }
    public int VeiculoId { get; set; }
}

public class CaptacaoCreateDto
{
    public string Nome      { get; set; } = "";
    public string Telefone  { get; set; } = "";
    public string Email     { get; set; } = "";
    public string Interesse { get; set; } = "";
    public string Orcamento { get; set; } = "";
    public int    LojaId    { get; set; } = 1;
}

public class LojaUpdateDto
{
    public string Nome     { get; set; } = "";
    public string Cnpj     { get; set; } = "";
    public string Telefone { get; set; } = "";
    public string Whatsapp { get; set; } = "";
    public string Email    { get; set; } = "";
    public string Endereco { get; set; } = "";
    public string Cidade   { get; set; } = "";
    public string Estado   { get; set; } = "";
    public string LogoUrl  { get; set; } = "";
    public string Plano    { get; set; } = "basico";
}

public class SimulacaoRequestDto
{
    public int     VeiculoId    { get; set; }
    public decimal ValorEntrada { get; set; }
    public int     PrazoMeses   { get; set; }
    public int?    ClienteId    { get; set; }
}

public class PropostaFinanciamentoDto
{
    public string  Banco        { get; set; } = "";
    public double  TaxaMensal   { get; set; }
    public int     Prazo        { get; set; }
    public decimal ValorParcela { get; set; }
    public decimal TotalPagar   { get; set; }
    public string  Status       { get; set; } = "";
    public bool    MelhorOferta { get; set; }
}

public class CrmStatsDto
{
    public int Leads     { get; set; }
    public int Quentes   { get; set; }
    public int Mornos    { get; set; }
    public int Frios     { get; set; }
    public int Captacoes { get; set; }
    public int Veiculos  { get; set; }
}

public class LeadDto
{
    public int      Id              { get; set; }
    public string   Nome            { get; set; } = "";
    public string   Email           { get; set; } = "";
    public string   Telefone        { get; set; } = "";
    public string   Interesse       { get; set; } = "";
    public decimal  Orcamento       { get; set; }
    public string   Origem          { get; set; } = "";
    public string   Status          { get; set; } = "";
    public DateTime UltimaInteracao { get; set; }
    public int      TotalVisitas    { get; set; }
    public List<VeiculoDto> VeiculosVisitados { get; set; } = [];
}

public class CompraEtapaDto
{
    public int EtapaAtual { get; set; }
}

public class IdResponse
{
    public int Id { get; set; }
}
