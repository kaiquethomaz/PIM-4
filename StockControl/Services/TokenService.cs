using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using StockControl.Dtos;
using StockControl.Extensions;
using StockControl.Models;

namespace StockControl.Services;

/// <summary>
/// Cria o token JWT (JSON Web Token).
/// O token tem 3 partes separadas por ponto: cabeçalho.dados.assinatura
///  - "dados" (claims): Id, nome, e-mail, perfil e validade do usuário;
///  - "assinatura": calculada com a Chave secreta (HMAC-SHA256). Se alguém
///    alterar os dados (ex.: trocar o perfil para Admin), a assinatura não
///    bate mais e a API recusa o token.
/// </summary>
public class TokenService : ITokenService
{
    private readonly ConfiguracaoJwt _configuracao;

    public TokenService(IOptions<ConfiguracaoJwt> configuracao)
    {
        _configuracao = configuracao.Value;
    }

    public LoginResposta GerarToken(Usuario usuario)
    {
        var expiraEm = DateTime.UtcNow.AddHours(_configuracao.ExpiracaoHoras);

        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuracao.Chave));

        var descricao = new SecurityTokenDescriptor
        {
            Issuer = _configuracao.Emissor,
            Audience = _configuracao.Publico,
            Expires = expiraEm,
            SigningCredentials = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256),
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("sub", usuario.Id.ToString()),          // "subject" = Id do usuário
                new Claim("name", usuario.Nome),
                new Claim("email", usuario.Email),
                new Claim("role", usuario.Perfil.ToString()),     // usado no [Authorize(Roles = ...)]
                // Estabelecimento do usuário: a API filtra todos os dados por ele.
                new Claim(ClaimsStockControl.EstabelecimentoId, usuario.EstabelecimentoId.ToString()),
                new Claim(ClaimsStockControl.Estabelecimento, usuario.Estabelecimento?.NomeFantasia ?? string.Empty),
                new Claim("jti", Guid.NewGuid().ToString())       // identificador único do token
            })
        };

        var token = new JsonWebTokenHandler().CreateToken(descricao);

        return new LoginResposta
        {
            Token = token,
            ExpiraEm = expiraEm,
            Usuario = UsuarioResumo.DeModelo(usuario)
        };
    }
}
