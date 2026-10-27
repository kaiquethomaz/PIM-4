namespace StockControl.Excecoes;

/// <summary>
/// Erro "esperado", causado por uma regra de negócio (ex.: estoque insuficiente,
/// código de barras repetido). A mensagem é amigável e pode ir direto para a tela.
/// Os Services lançam esta exceção; os Controllers capturam e mostram ao usuário.
/// </summary>
public class RegraNegocioException : Exception
{
    public RegraNegocioException(string mensagem) : base(mensagem)
    {
    }
}
