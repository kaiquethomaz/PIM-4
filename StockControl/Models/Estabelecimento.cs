namespace StockControl.Models;

/// <summary>
/// Estabelecimento (mercadinho) que usa o sistema (tabela Estabelecimento).
/// Cada estabelecimento tem os próprios usuários, produtos, categorias e
/// fornecedores, e só enxerga os próprios dados.
/// </summary>
public class Estabelecimento : EntidadeBase
{
    public string NomeFantasia { get; set; } = string.Empty;

    // Só os 14 números (ex.: 12345678000195).
    public string CNPJ { get; set; } = string.Empty;

    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.Now;

    public string CnpjFormatado() => Cnpj.Formatar(CNPJ);
}
