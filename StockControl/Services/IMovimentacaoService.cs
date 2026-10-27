using StockControl.Models;
using StockControl.Models.Consultas;

namespace StockControl.Services;

/// <summary>Regras de negócio de Movimentação (entradas e vendas).</summary>
public interface IMovimentacaoService
{
    Task<Movimentacao> RegistrarAsync(int produtoId, TipoMovimentacao tipo, int quantidade, int usuarioId, string? observacao);
    Task<List<MovimentacaoRelatorio>> ListarPorPeriodoAsync(DateTime dataInicio, DateTime dataFim);
    Task<List<Movimentacao>> ListarUltimasAsync(int quantidade = 10);
}
