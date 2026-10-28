using System.Globalization;
using StockControl.DAO;
using StockControl.Models.Consultas;

namespace StockControl.Services;

public class AuditoriaService : IAuditoriaService
{
    private static readonly CultureInfo Brasil = new("pt-BR");

    private readonly ILogAuditoriaDAO _logDAO;

    public AuditoriaService(ILogAuditoriaDAO logDAO)
    {
        _logDAO = logDAO;
    }

    public async Task<List<RegistroAuditoria>> ListarAsync(int quantidade = 200)
    {
        var logs = await _logDAO.ListarRecentesAsync(quantidade);
        var nomes = await _logDAO.BuscarNomesDeProdutosAsync(logs.Select(l => l.RegistroId));

        return logs.Select(log =>
        {
            var excluido = log.Acao == "DELETE";
            return new RegistroAuditoria
            {
                Data = log.Data,
                Acao = log.Acao,
                Descricao = excluido ? "Exclusão de produto" : "Alteração de preço",
                ProdutoId = log.RegistroId,
                // Produto excluído não existe mais: o nome vem do próprio log ("Nome=...;").
                Produto = nomes.TryGetValue(log.RegistroId, out var nome)
                    ? nome
                    : ExtrairCampo(log.ValorAntigo, "Nome") ?? $"Produto #{log.RegistroId}",
                Antes = excluido ? DescreverExclusao(log.ValorAntigo) : FormatarPreco(log.ValorAntigo),
                Depois = excluido ? "(excluído)" : FormatarPreco(log.ValorNovo)
            };
        }).ToList();
    }

    /// <summary>A trigger grava o preço com ponto ("28.90"); aqui vira "R$ 28,90".</summary>
    private static string FormatarPreco(string? valor)
    {
        return decimal.TryParse(valor, NumberStyles.Number, CultureInfo.InvariantCulture, out var preco)
            ? preco.ToString("C", Brasil)
            : valor ?? string.Empty;
    }

    /// <summary>"Nome=Arroz; CodigoBarras=789...; Preco=28.90; Estoque=0" -> "Código 789... · R$ 28,90 · estoque 0"</summary>
    private static string DescreverExclusao(string? valor)
    {
        var codigo = ExtrairCampo(valor, "CodigoBarras");
        var preco = FormatarPreco(ExtrairCampo(valor, "Preco"));
        var estoque = ExtrairCampo(valor, "Estoque");
        return $"Código {codigo} · {preco} · estoque {estoque}";
    }

    private static string? ExtrairCampo(string? texto, string campo)
    {
        if (string.IsNullOrEmpty(texto)) return null;
        foreach (var parte in texto.Split(';'))
        {
            var chaveValor = parte.Split('=', 2);
            if (chaveValor.Length == 2 && chaveValor[0].Trim() == campo)
                return chaveValor[1].Trim();
        }
        return null;
    }
}
