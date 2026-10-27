using System.ComponentModel.DataAnnotations;
using StockControl.Models;

namespace StockControl.ViewModels;

/// <summary>Formulário de usuário (criar e editar). Só o Admin acessa.</summary>
public class UsuarioFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(100, ErrorMessage = "O nome pode ter no máximo {1} caracteres.")]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [StringLength(150, ErrorMessage = "O e-mail pode ter no máximo {1} caracteres.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Perfil")]
    public PerfilUsuario Perfil { get; set; } = PerfilUsuario.Operador;

    [Display(Name = "Usuário ativo")]
    public bool Ativo { get; set; } = true;

    // No cadastro a senha é obrigatória (conferido no controller);
    // na edição, deixar em branco mantém a senha atual.
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter entre {2} e {1} caracteres.")]
    [Display(Name = "Senha")]
    public string? Senha { get; set; }

    [DataType(DataType.Password)]
    [Compare(nameof(Senha), ErrorMessage = "As senhas não conferem.")]
    [Display(Name = "Confirmar senha")]
    public string? ConfirmarSenha { get; set; }

    public bool EhNovo => Id == 0;

    public static UsuarioFormViewModel DeModelo(Usuario usuario) => new()
    {
        Id = usuario.Id,
        Nome = usuario.Nome,
        Email = usuario.Email,
        Perfil = usuario.Perfil,
        Ativo = usuario.Ativo
    };

    public Usuario ParaModelo() => new()
    {
        Id = Id,
        Nome = Nome,
        Email = Email,
        Perfil = Perfil,
        Ativo = Ativo
    };
}
