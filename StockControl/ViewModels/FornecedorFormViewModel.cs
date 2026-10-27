using System.ComponentModel.DataAnnotations;
using StockControl.Models;

namespace StockControl.ViewModels;

/// <summary>Dados do formulário de fornecedor (criar e editar).</summary>
public class FornecedorFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe a razão social.")]
    [StringLength(150, ErrorMessage = "A razão social pode ter no máximo {1} caracteres.")]
    [Display(Name = "Razão social")]
    public string RazaoSocial { get; set; } = string.Empty;

    // Aceita com ou sem máscara; o Service tira pontos, barra e traço.
    [Required(ErrorMessage = "Informe o CNPJ.")]
    [StringLength(18, MinimumLength = 14, ErrorMessage = "CNPJ inválido.")]
    [Display(Name = "CNPJ")]
    public string CNPJ { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "O telefone pode ter no máximo {1} caracteres.")]
    [Phone(ErrorMessage = "Telefone inválido.")]
    [Display(Name = "Telefone")]
    public string? Telefone { get; set; }

    [StringLength(150, ErrorMessage = "O e-mail pode ter no máximo {1} caracteres.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [Display(Name = "E-mail")]
    public string? Email { get; set; }

    public bool EhNovo => Id == 0;

    public static FornecedorFormViewModel DeModelo(Fornecedor fornecedor) => new()
    {
        Id = fornecedor.Id,
        RazaoSocial = fornecedor.RazaoSocial,
        CNPJ = fornecedor.CnpjFormatado(),
        Telefone = fornecedor.Telefone,
        Email = fornecedor.Email
    };

    public Fornecedor ParaModelo() => new()
    {
        Id = Id,
        RazaoSocial = RazaoSocial,
        CNPJ = CNPJ,
        Telefone = Telefone,
        Email = Email
    };
}
