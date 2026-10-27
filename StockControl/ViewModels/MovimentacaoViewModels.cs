using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using StockControl.Models;
using StockControl.Models.Consultas;

namespace StockControl.ViewModels;

/// <summary>Formulário de registro de entrada ou venda (saída).</summary>
public class MovimentacaoFormViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Selecione o produto.")]
    [Display(Name = "Produto")]
    public int ProdutoId { get; set; }

    // Nullable + [Required]: obriga o usuário a escolher Entrada ou Saída.
    [Required(ErrorMessage = "Escolha o tipo da movimentação.")]
    [Display(Name = "Tipo")]
    public TipoMovimentacao? Tipo { get; set; }

    [Required(ErrorMessage = "Informe a quantidade.")]
    [Range(1, 1_000_000, ErrorMessage = "A quantidade deve ser maior que zero.")]
    [Display(Name = "Quantidade")]
    public int? Quantidade { get; set; }

    [StringLength(250, ErrorMessage = "A observação pode ter no máximo {1} caracteres.")]
    [Display(Name = "Observação")]
    public string? Observacao { get; set; }

    // Opções do <select> de produtos (com o estoque atual no texto).
    [ValidateNever]
    public List<SelectListItem> Produtos { get; set; } = new();
}

/// <summary>Listagem de movimentações filtrada por período.</summary>
public class MovimentacaoListaViewModel
{
    [Display(Name = "Data inicial")]
    public DateTime DataInicio { get; set; }

    [Display(Name = "Data final")]
    public DateTime DataFim { get; set; }

    public List<MovimentacaoRelatorio> Itens { get; set; } = new();

    /// <summary>Mensagem de erro do filtro (ex.: data inicial maior que a final).</summary>
    public string? Erro { get; set; }

    // Totais do período (somam as quantidades, não o número de registros).
    public int TotalEntradas => Itens.Where(i => i.Tipo == "ENTRADA").Sum(i => i.Quantidade);
    public int TotalSaidas => Itens.Where(i => i.Tipo == "SAIDA").Sum(i => i.Quantidade);
}
