using System.ComponentModel.DataAnnotations;
using StockControl.Models;

namespace StockControl.Dtos;

// DTO = Data Transfer Object: objeto que só carrega dados entre a API e o app.
// A API nunca devolve as entidades direto (ex.: Usuario tem SenhaHash, que
// não pode sair do servidor).

/// <summary>Dados enviados pelo app para fazer login.</summary>
public class LoginRequisicao
{
    /// <example>operador@stockcontrol.local</example>
    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    public string Email { get; set; } = string.Empty;

    /// <example>Operador@123</example>
    [Required(ErrorMessage = "Informe a senha.")]
    public string Senha { get; set; } = string.Empty;
}

/// <summary>Resposta do login: o token e os dados do usuário.</summary>
public class LoginResposta
{
    /// <summary>Token JWT. Enviar em todas as requisições no cabeçalho: Authorization: Bearer {token}</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>Data/hora (UTC) em que o token expira; depois disso é preciso fazer login de novo.</summary>
    public DateTime ExpiraEm { get; set; }

    public UsuarioResumo Usuario { get; set; } = new();
}

/// <summary>Dados públicos do usuário (sem a senha).</summary>
public class UsuarioResumo
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    /// <summary>Admin, Gerente ou Operador.</summary>
    public string Perfil { get; set; } = string.Empty;

    public static UsuarioResumo DeModelo(Usuario usuario) => new()
    {
        Id = usuario.Id,
        Nome = usuario.Nome,
        Email = usuario.Email,
        Perfil = usuario.Perfil.ToString()
    };
}

/// <summary>Resposta quando o JSON enviado tem campos inválidos (HTTP 400).</summary>
public class ErroValidacaoResposta
{
    /// <example>Dados inválidos.</example>
    public string Mensagem { get; set; } = "Dados inválidos.";

    /// <summary>Erros por campo. Ex.: { "Nome": ["Informe o nome."] }</summary>
    public Dictionary<string, string[]> Erros { get; set; } = new();
}

/// <summary>Formato padrão das respostas de erro e de aviso da API.</summary>
public class MensagemResposta
{
    public string Mensagem { get; set; } = string.Empty;

    public MensagemResposta() { }
    public MensagemResposta(string mensagem) => Mensagem = mensagem;
}
