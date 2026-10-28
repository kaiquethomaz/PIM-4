namespace StockControl.Models.Consultas;

/// <summary>Uma linha da tela de Auditoria, já pronta para exibir.</summary>
public class RegistroAuditoria
{
    public DateTime Data { get; set; }

    /// <summary>"UPDATE_PRECO" ou "DELETE" (como gravado pela trigger).</summary>
    public string Acao { get; set; } = string.Empty;

    /// <summary>Texto amigável: "Alteração de preço" / "Exclusão".</summary>
    public string Descricao { get; set; } = string.Empty;

    public int ProdutoId { get; set; }
    public string Produto { get; set; } = string.Empty;
    public string Antes { get; set; } = string.Empty;
    public string Depois { get; set; } = string.Empty;
}
