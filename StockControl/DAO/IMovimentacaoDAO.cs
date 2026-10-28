using StockControl.Models;
using StockControl.Models.Consultas;

namespace StockControl.DAO;

/// <summary>
/// Acesso a dados da tabela Movimentacao.
/// Não herda IDAOBase: movimentação não se edita nem se exclui (é histórico).
/// </summary>
public interface IMovimentacaoDAO
{
    /// <summary>Executa sp_RegistrarMovimentacao e devolve o Id criado.</summary>
    Task<int> RegistrarAsync(Movimentacao movimentacao);

    /// <summary>Executa sp_RelatorioMovimentacoes (os dois dias inclusos).</summary>
    Task<List<MovimentacaoRelatorio>> ListarPorPeriodoAsync(DateTime dataInicio, DateTime dataFim);

    Task<List<Movimentacao>> ListarUltimasAsync(int quantidade);

    /// <summary>Quantidade de movimentações de um tipo a partir de uma data.</summary>
    Task<int> ContarDesdeAsync(TipoMovimentacao tipo, DateTime desde);

    /// <summary>Vendas (saídas) agrupadas por dia, em R$ e em itens, a partir de uma data.</summary>
    Task<List<VendaDia>> SomarVendasPorDiaAsync(DateTime desde);
}
