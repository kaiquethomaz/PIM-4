using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockControl.Dtos;
using StockControl.Extensions;
using StockControl.Services;

namespace StockControl.Controllers.Api;

/// <summary>Login do app mobile.</summary>
[Route("api/auth")]
public class AuthController : ApiControllerBase
{
    private readonly IUsuarioService _usuarioService;
    private readonly ITokenService _tokenService;

    public AuthController(IUsuarioService usuarioService, ITokenService tokenService)
    {
        _usuarioService = usuarioService;
        _tokenService = tokenService;
    }

    /// <summary>Faz login e devolve o token JWT.</summary>
    /// <remarks>
    /// Use o token recebido em todas as outras requisições, no cabeçalho:
    /// `Authorization: Bearer {token}`
    /// </remarks>
    /// <response code="200">Login feito; devolve o token e os dados do usuário.</response>
    /// <response code="400">E-mail ou senha não informados.</response>
    /// <response code="401">E-mail ou senha inválidos, ou usuário inativo.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResposta), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensagemResposta), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResposta>> Login(LoginRequisicao requisicao)
    {
        var usuario = await _usuarioService.AutenticarAsync(requisicao.Email, requisicao.Senha);
        if (usuario is null)
            return Unauthorized(new MensagemResposta("E-mail ou senha inválidos."));

        return Ok(_tokenService.GerarToken(usuario));
    }

    /// <summary>Dados do usuário dono do token (serve para o app conferir se o token ainda vale).</summary>
    /// <response code="200">Token válido.</response>
    /// <response code="401">Token ausente, inválido ou expirado.</response>
    [HttpGet("eu")]
    [ProducesResponseType(typeof(UsuarioResumo), StatusCodes.Status200OK)]
    public async Task<ActionResult<UsuarioResumo>> Eu()
    {
        var usuario = await _usuarioService.ObterAsync(User.ObterUsuarioId());
        if (usuario is null || !usuario.Ativo)
            return Unauthorized(new MensagemResposta("Usuário inexistente ou inativo."));

        return Ok(UsuarioResumo.DeModelo(usuario));
    }
}
