using StockControl.Models;

namespace StockControl.DAO;

/// <summary>
/// Leitura da tabela LogAuditoria (quem grava é a trigger trg_AuditoriaProduto;
/// o sistema só consulta).
/// </summary>
public interface ILogAuditoriaDAO
{
    Task<List<LogAuditoria>> ListarRecentesAsync(int quantidade);

    /// <summary>Nome dos produtos ainda existentes, por Id (para mostrar no histórico).</summary>
    Task<Dictionary<int, string>> BuscarNomesDeProdutosAsync(IEnumerable<int> ids);
}
