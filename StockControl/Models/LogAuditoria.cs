namespace StockControl.Models;

/// <summary>
/// Registro de auditoria (tabela LogAuditoria). Quem grava é a trigger
/// trg_AuditoriaProduto; o sistema só lê.
/// </summary>
public class LogAuditoria
{
    public int Id { get; set; }
    public string Tabela { get; set; } = string.Empty;
    public string Acao { get; set; } = string.Empty;
    public int RegistroId { get; set; }
    public string? ValorAntigo { get; set; }
    public string? ValorNovo { get; set; }
    public DateTime Data { get; set; }
}
