using StockControl.Dtos;
using StockControl.Models;

namespace StockControl.Services;

/// <summary>Gera o token JWT usado pelo app mobile.</summary>
public interface ITokenService
{
    LoginResposta GerarToken(Usuario usuario);
}
