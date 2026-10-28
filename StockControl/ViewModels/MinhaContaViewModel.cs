using System.ComponentModel.DataAnnotations;

namespace StockControl.ViewModels;

/// <summary>Tela "Minha conta": dados do usuário logado (só leitura) + troca da própria senha.</summary>
public class MinhaContaViewModel
{
    // Só para exibir
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Perfil { get; set; } = string.Empty;
    public string Estabelecimento { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a senha atual.")]
    [DataType(DataType.Password)]
    [Display(Name = "Senha atual")]
    public string SenhaAtual { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a nova senha.")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter entre {2} e {1} caracteres.")]
    [Display(Name = "Nova senha")]
    public string NovaSenha { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirme a nova senha.")]
    [DataType(DataType.Password)]
    [Compare(nameof(NovaSenha), ErrorMessage = "As senhas não conferem.")]
    [Display(Name = "Confirmar nova senha")]
    public string ConfirmarSenha { get; set; } = string.Empty;
}
