namespace StockControl.Models;

/// <summary>
/// Entrada ou saída de estoque (tabela Movimentacao).
/// Não herda de EntidadeBase porque uma movimentação é um registro histórico:
/// depois de gravada ela nunca é alterada, então não tem "AtualizadoEm".
/// </summary>
public class Movimentacao
{
    public int Id { get; set; }

    public int ProdutoId { get; set; }
    public Produto? Produto { get; set; }

    public TipoMovimentacao Tipo { get; set; }
    public int Quantidade { get; set; }
    public DateTime Data { get; set; } = DateTime.Now;

    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public string? Observacao { get; set; }
}
