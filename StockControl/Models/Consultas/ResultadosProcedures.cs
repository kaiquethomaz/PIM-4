namespace StockControl.Models.Consultas;

// Classes que recebem o resultado das procedures.
// Elas não são tabelas: os nomes das propriedades precisam ser IGUAIS
// aos nomes das colunas do SELECT da procedure.

/// <summary>Uma linha de sp_ProdutosAbaixoDoMinimo.</summary>
public class ProdutoAbaixoMinimo
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string CodigoBarras { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public int EstoqueAtual { get; set; }
    public int EstoqueMinimo { get; set; }
    public int QuantidadeFaltante { get; set; }
}

/// <summary>Uma linha de sp_RelatorioMovimentacoes.</summary>
public class MovimentacaoRelatorio
{
    public int Id { get; set; }
    public DateTime Data { get; set; }
    public string Tipo { get; set; } = string.Empty;   // 'ENTRADA' ou 'SAIDA'
    public int ProdutoId { get; set; }
    public string Produto { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public int UsuarioId { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string? Observacao { get; set; }
}

/// <summary>Retorno de sp_RegistrarMovimentacao (SELECT @NovoId AS Id).</summary>
public class IdGerado
{
    public int Id { get; set; }
}
