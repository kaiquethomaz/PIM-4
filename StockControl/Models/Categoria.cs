namespace StockControl.Models;

/// <summary>Categoria de produtos (tabela Categoria).</summary>
public class Categoria : EntidadeBase
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }

    // Navegação: produtos desta categoria (relacionamento 1:N)
    public ICollection<Produto> Produtos { get; set; } = new List<Produto>();
}
