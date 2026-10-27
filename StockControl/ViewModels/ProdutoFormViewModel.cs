using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using StockControl.Models;

namespace StockControl.ViewModels;

/// <summary>Dados do formulário de produto (criar e editar).</summary>
public class ProdutoFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(120, ErrorMessage = "O nome pode ter no máximo {1} caracteres.")]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o código de barras.")]
    [RegularExpression(@"^\d{1,20}$", ErrorMessage = "O código de barras deve ter só números (até 20).")]
    [Display(Name = "Código de barras")]
    public string CodigoBarras { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o preço.")]
    [Range(typeof(decimal), "0", "99999999", ErrorMessage = "O preço deve ser maior ou igual a zero.")]
    [Display(Name = "Preço (R$)")]
    public decimal? Preco { get; set; }

    [Required(ErrorMessage = "Informe o estoque mínimo.")]
    [Range(0, 1_000_000, ErrorMessage = "O estoque mínimo deve ser zero ou maior.")]
    [Display(Name = "Estoque mínimo")]
    public int? EstoqueMinimo { get; set; }

    // Usado só no cadastro: vira uma movimentação de ENTRADA.
    [Range(0, 1_000_000, ErrorMessage = "O estoque inicial deve ser zero ou maior.")]
    [Display(Name = "Estoque inicial")]
    public int EstoqueInicial { get; set; }

    // Só para exibir na edição (não é enviado para o banco).
    public int EstoqueAtual { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecione a categoria.")]
    [Display(Name = "Categoria")]
    public int CategoriaId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecione o fornecedor.")]
    [Display(Name = "Fornecedor")]
    public int FornecedorId { get; set; }

    [Display(Name = "Produto ativo")]
    public bool Ativo { get; set; } = true;

    // Opções dos <select>. [ValidateNever]: não são dados digitados, não validar.
    [ValidateNever]
    public List<SelectListItem> Categorias { get; set; } = new();

    [ValidateNever]
    public List<SelectListItem> Fornecedores { get; set; } = new();

    public bool EhNovo => Id == 0;

    public static ProdutoFormViewModel DeModelo(Produto produto) => new()
    {
        Id = produto.Id,
        Nome = produto.Nome,
        CodigoBarras = produto.CodigoBarras,
        Preco = produto.Preco,
        EstoqueMinimo = produto.EstoqueMinimo,
        EstoqueAtual = produto.EstoqueAtual,
        CategoriaId = produto.CategoriaId,
        FornecedorId = produto.FornecedorId,
        Ativo = produto.Ativo
    };

    public Produto ParaModelo() => new()
    {
        Id = Id,
        Nome = Nome,
        CodigoBarras = CodigoBarras,
        Preco = Preco ?? 0,
        EstoqueMinimo = EstoqueMinimo ?? 0,
        CategoriaId = CategoriaId,
        FornecedorId = FornecedorId,
        Ativo = Ativo
    };
}
