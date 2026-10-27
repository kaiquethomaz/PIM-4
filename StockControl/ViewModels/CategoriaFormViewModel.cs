using System.ComponentModel.DataAnnotations;
using StockControl.Models;

namespace StockControl.ViewModels;

/// <summary>
/// Dados do formulário de categoria (criar e editar).
/// Usamos ViewModel em vez da entidade direto na tela para controlar
/// exatamente quais campos o usuário pode enviar.
/// </summary>
public class CategoriaFormViewModel
{
    public int Id { get; set; }   // 0 = nova categoria

    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(60, ErrorMessage = "O nome pode ter no máximo {1} caracteres.")]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "A descrição pode ter no máximo {1} caracteres.")]
    [Display(Name = "Descrição")]
    public string? Descricao { get; set; }

    public bool EhNovo => Id == 0;

    public static CategoriaFormViewModel DeModelo(Categoria categoria) => new()
    {
        Id = categoria.Id,
        Nome = categoria.Nome,
        Descricao = categoria.Descricao
    };

    public Categoria ParaModelo() => new()
    {
        Id = Id,
        Nome = Nome,
        Descricao = Descricao
    };
}
