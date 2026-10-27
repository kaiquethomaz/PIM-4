namespace StockControl.Models;

/// <summary>
/// Tipo da movimentação. No banco fica como 'ENTRADA' ou 'SAIDA'
/// (a conversão está em MovimentacaoConfiguration).
/// Uma venda é uma movimentação do tipo Saida.
/// </summary>
public enum TipoMovimentacao
{
    Entrada,
    Saida
}
