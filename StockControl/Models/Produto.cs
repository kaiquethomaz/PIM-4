namespace StockControl.Models;

/// <summary>Produto vendido no mercadinho (tabela Produto).</summary>
public class Produto : EntidadeBase
{
    public string Nome { get; set; } = string.Empty;
    public string CodigoBarras { get; set; } = string.Empty;
    public decimal Preco { get; set; }

    // Encapsulamento: o estoque NÃO pode ser alterado pelo código C#.
    // Quem altera é a trigger trg_AtualizaEstoque, quando uma movimentação
    // é registrada. Por isso o "set" é privado (só o EF consegue preencher
    // o valor ao ler do banco).
    public int EstoqueAtual { get; private set; }

    public int EstoqueMinimo { get; set; }

    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    public int FornecedorId { get; set; }
    public Fornecedor? Fornecedor { get; set; }

    public bool Ativo { get; set; } = true;

    public ICollection<Movimentacao> Movimentacoes { get; set; } = new List<Movimentacao>();

    /// <summary>True quando o produto precisa de reposição (aparece no dashboard).</summary>
    public bool EstaAbaixoDoMinimo() => EstoqueAtual < EstoqueMinimo;

    /// <summary>True quando há estoque suficiente para vender a quantidade pedida.</summary>
    public bool TemEstoquePara(int quantidade) => quantidade <= EstoqueAtual;
}
