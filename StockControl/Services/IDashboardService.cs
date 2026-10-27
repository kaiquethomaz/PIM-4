using StockControl.Models.Consultas;

namespace StockControl.Services;

/// <summary>Monta os números do dashboard (web e API).</summary>
public interface IDashboardService
{
    Task<ResumoDashboard> ObterResumoAsync();
}
