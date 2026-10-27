namespace StockControl.Services;

/// <summary>
/// Configurações do token JWT, lidas da seção "Jwt" do appsettings.json.
/// Em produção, a Chave deve vir de variável de ambiente (Jwt__Chave),
/// nunca ficar gravada no código.
/// </summary>
public class ConfiguracaoJwt
{
    public string Emissor { get; set; } = string.Empty;   // quem gera o token (issuer)
    public string Publico { get; set; } = string.Empty;   // para quem o token é (audience)
    public string Chave { get; set; } = string.Empty;     // segredo usado na assinatura
    public int ExpiracaoHoras { get; set; } = 8;
}
