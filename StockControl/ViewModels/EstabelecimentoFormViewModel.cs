using System.ComponentModel.DataAnnotations;
using StockControl.Models;

namespace StockControl.ViewModels;

/// <summary>Tela "Meu estabelecimento" (só o Admin). O CNPJ aparece, mas não pode ser alterado.</summary>
public class EstabelecimentoFormViewModel
{
    [Required(ErrorMessage = "Informe o nome do estabelecimento.")]
    [StringLength(120, ErrorMessage = "O nome pode ter no máximo {1} caracteres.")]
    [Display(Name = "Nome do estabelecimento")]
    public string NomeFantasia { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Telefone inválido.")]
    [StringLength(20, ErrorMessage = "O telefone pode ter no máximo {1} caracteres.")]
    [Display(Name = "Telefone")]
    public string? Telefone { get; set; }

    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [StringLength(150, ErrorMessage = "O e-mail pode ter no máximo {1} caracteres.")]
    [Display(Name = "E-mail de contato")]
    public string? Email { get; set; }

    // Só para exibir.
    public string CnpjFormatado { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; }

    public static EstabelecimentoFormViewModel DeModelo(Estabelecimento e) => new()
    {
        NomeFantasia = e.NomeFantasia,
        Telefone = e.Telefone,
        Email = e.Email,
        CnpjFormatado = e.CnpjFormatado(),
        CriadoEm = e.CriadoEm
    };

    public Estabelecimento ParaModelo() => new()
    {
        NomeFantasia = NomeFantasia,
        Telefone = Telefone,
        Email = Email
    };
}
