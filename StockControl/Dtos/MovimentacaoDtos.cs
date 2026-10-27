using System.ComponentModel.DataAnnotations;
using StockControl.Models;
using StockControl.Models.Consultas;

namespace StockControl.Dtos;

/// <summary>
/// Registro rápido de entrada ou venda pelo app.
/// Informe o ProdutoId OU o CodigoBarras (lido pela câmera do celular).
/// </summary>
public class MovimentacaoRequisicao
{
    /// <example>1</example>
    public int? ProdutoId { get; set; }

    /// <example>7891000000011</example>
    public string? CodigoBarras { get; set; }

    /// <summary>ENTRADA ou SAIDA (venda).</summary>
    /// <example>SAIDA</example>
    [Required(ErrorMessage = "Informe o tipo (ENTRADA ou SAIDA).")]
    [RegularExpression("^(?i)(ENTRADA|SAIDA)$", ErrorMessage = "O tipo deve ser ENTRADA ou SAIDA.")]
    public string Tipo { get; set; } = string.Empty;

    /// <example>2</example>
    [Range(1, 1_000_000, ErrorMessage = "A quantidade deve ser maior que zero.")]
    public int Quantidade { get; set; }

    /// <example>Venda pelo app</example>
    [StringLength(250, ErrorMessage = "A observação pode ter no máximo {1} caracteres.")]
    public string? Observacao { get; set; }

    public TipoMovimentacao TipoConvertido() =>
        Tipo.Trim().ToUpper() == "ENTRADA" ? TipoMovimentacao.Entrada : TipoMovimentacao.Saida;
}

/// <summary>Movimentação devolvida pela API.</summary>
public class MovimentacaoDto
{
    public int Id { get; set; }
    public DateTime Data { get; set; }

    /// <summary>ENTRADA ou SAIDA.</summary>
    public string Tipo { get; set; } = string.Empty;

    public int ProdutoId { get; set; }
    public string Produto { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public int UsuarioId { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string? Observacao { get; set; }

    public static MovimentacaoDto DeModelo(Movimentacao m) => new()
    {
        Id = m.Id,
        Data = m.Data,
        Tipo = m.Tipo == TipoMovimentacao.Entrada ? "ENTRADA" : "SAIDA",
        ProdutoId = m.ProdutoId,
        Produto = m.Produto?.Nome ?? string.Empty,
        Quantidade = m.Quantidade,
        UsuarioId = m.UsuarioId,
        Usuario = m.Usuario?.Nome ?? string.Empty,
        Observacao = m.Observacao
    };

    public static MovimentacaoDto DeRelatorio(MovimentacaoRelatorio m) => new()
    {
        Id = m.Id,
        Data = m.Data,
        Tipo = m.Tipo,
        ProdutoId = m.ProdutoId,
        Produto = m.Produto,
        Quantidade = m.Quantidade,
        UsuarioId = m.UsuarioId,
        Usuario = m.Usuario,
        Observacao = m.Observacao
    };
}

/// <summary>Resposta do registro de movimentação: a movimentação e o produto com o estoque já atualizado.</summary>
public class MovimentacaoRegistradaDto
{
    public MovimentacaoDto Movimentacao { get; set; } = new();
    public ProdutoDto Produto { get; set; } = new();
}

/// <summary>Resumo do dashboard para o app.</summary>
public class DashboardResumoDto
{
    public int TotalProdutosAtivos { get; set; }
    public int TotalCategorias { get; set; }
    public int TotalFornecedores { get; set; }
    public decimal ValorEmEstoque { get; set; }
    public int EntradasHoje { get; set; }
    public int SaidasHoje { get; set; }
    public List<ProdutoAbaixoMinimo> ProdutosAbaixoDoMinimo { get; set; } = new();
    public List<MovimentacaoDto> UltimasMovimentacoes { get; set; } = new();

    public static DashboardResumoDto DeModelo(ResumoDashboard r) => new()
    {
        TotalProdutosAtivos = r.TotalProdutosAtivos,
        TotalCategorias = r.TotalCategorias,
        TotalFornecedores = r.TotalFornecedores,
        ValorEmEstoque = r.ValorEmEstoque,
        EntradasHoje = r.EntradasHoje,
        SaidasHoje = r.SaidasHoje,
        ProdutosAbaixoDoMinimo = r.ProdutosAbaixoDoMinimo,
        UltimasMovimentacoes = r.UltimasMovimentacoes.Select(MovimentacaoDto.DeModelo).ToList()
    };
}
