using StockControl.Models.Consultas;

namespace StockControl.Services;

/// <summary>Histórico de alterações de preço e exclusões de produtos (tela de Auditoria).</summary>
public interface IAuditoriaService
{
    Task<List<RegistroAuditoria>> ListarAsync(int quantidade = 200);
}
