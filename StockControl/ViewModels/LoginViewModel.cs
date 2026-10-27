using System.ComponentModel.DataAnnotations;

namespace StockControl.ViewModels;

/// <summary>Dados da tela de login.</summary>
public class LoginViewModel
{
    // DataAnnotations: regras de validação lidas pelo ModelState.
    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a senha.")]
    [DataType(DataType.Password)]
    [Display(Name = "Senha")]
    public string Senha { get; set; } = string.Empty;

    [Display(Name = "Manter conectado")]
    public bool LembrarDeMim { get; set; }

    /// <summary>Página que o usuário tentou abrir antes de ir para o login.</summary>
    public string? ReturnUrl { get; set; }
}
