namespace StockControl.Models;

/// <summary>
/// Interface das entidades que pertencem a um estabelecimento (mercadinho):
/// Usuario, Categoria, Fornecedor e Produto.
/// O AppDbContext usa esta interface para preencher o EstabelecimentoId
/// automaticamente ao salvar um registro novo.
/// </summary>
public interface IPertenceAEstabelecimento
{
    int EstabelecimentoId { get; set; }
}
