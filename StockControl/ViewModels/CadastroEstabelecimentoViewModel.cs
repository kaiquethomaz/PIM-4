using System.ComponentModel.DataAnnotations;
using StockControl.Models;

namespace StockControl.ViewModels;

/// <summary>
/// Tela "Cadastrar meu estabelecimento": dados do mercadinho + dados do dono,
/// que vira o primeiro usuário (Admin) do estabelecimento.
/// </summary>
public class CadastroEstabelecimentoViewModel
{
    // ---- Estabelecimento ----
    [Required(ErrorMessage = "Informe o nome do estabelecimento.")]
    [StringLength(120, ErrorMessage = "O nome pode ter no máximo {1} caracteres.")]
    [Display(Name = "Nome do estabelecimento")]
    public string NomeFantasia { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o CNPJ.")]
    [StringLength(18, MinimumLength = 14, ErrorMessage = "CNPJ inválido.")]
    [Display(Name = "CNPJ")]
    public string CNPJ { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Telefone inválido.")]
    [StringLength(20, ErrorMessage = "O telefone pode ter no máximo {1} caracteres.")]
    [Display(Name = "Telefone")]
    public string? Telefone { get; set; }

    // ---- Dono (Admin) ----
    [Required(ErrorMessage = "Informe o seu nome.")]
    [StringLength(100, ErrorMessage = "O nome pode ter no máximo {1} caracteres.")]
    [Display(Name = "Seu nome")]
    public string NomeResponsavel { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [StringLength(150, ErrorMessage = "O e-mail pode ter no máximo {1} caracteres.")]
    [Display(Name = "E-mail (será o seu login)")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a senha.")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter entre {2} e {1} caracteres.")]
    [Display(Name = "Senha")]
    public string Senha { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirme a senha.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Senha), ErrorMessage = "As senhas não conferem.")]
    [Display(Name = "Confirmar senha")]
    public string ConfirmarSenha { get; set; } = string.Empty;

    public Estabelecimento ParaEstabelecimento() => new()
    {
        NomeFantasia = NomeFantasia,
        CNPJ = CNPJ,
        Telefone = Telefone,
        Email = Email
    };

    public Usuario ParaDono() => new()
    {
        Nome = NomeResponsavel,
        Email = Email
    };
}
