using Microsoft.AspNetCore.Html;

namespace StockControl.Extensions;

/// <summary>
/// Ícones SVG do sistema (desenho de linha, mesmo estilo do logo).
/// Uso nas Views: @Icones.Svg("produtos")
/// SVG embutido no HTML: não depende de biblioteca externa nem de internet,
/// fica nítido em qualquer tamanho e herda a cor do texto (currentColor).
/// Todos levam aria-hidden="true": são decorativos, o texto ao lado é que dá o significado.
/// </summary>
public static class Icones
{
    private static readonly Dictionary<string, string> Desenhos = new()
    {
        ["dashboard"] = "<rect x='3' y='3' width='7' height='9' rx='1'/><rect x='14' y='3' width='7' height='5' rx='1'/><rect x='14' y='12' width='7' height='9' rx='1'/><rect x='3' y='16' width='7' height='5' rx='1'/>",
        ["movimentacoes"] = "<polyline points='17 1 21 5 17 9'/><path d='M3 11V9a4 4 0 0 1 4-4h14'/><polyline points='7 23 3 19 7 15'/><path d='M21 13v2a4 4 0 0 1-4 4H3'/>",
        ["produtos"] = "<path d='M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z'/><polyline points='3.27 6.96 12 12.01 20.73 6.96'/><line x1='12' y1='22.08' x2='12' y2='12'/>",
        ["categorias"] = "<path d='M20.59 13.41l-7.17 7.17a2 2 0 0 1-2.83 0L2 12V2h10l8.59 8.59a2 2 0 0 1 0 2.82z'/><line x1='7' y1='7' x2='7.01' y2='7'/>",
        ["fornecedores"] = "<rect x='1' y='3' width='15' height='13'/><polygon points='16 8 20 8 23 11 23 16 16 16 16 8'/><circle cx='5.5' cy='18.5' r='2.5'/><circle cx='18.5' cy='18.5' r='2.5'/>",
        ["usuarios"] = "<path d='M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2'/><circle cx='9' cy='7' r='4'/><path d='M23 21v-2a4 4 0 0 0-3-3.87'/><path d='M16 3.13a4 4 0 0 1 0 7.75'/>",
        ["estabelecimento"] = "<path d='M3 9l1-5h16l1 5'/><path d='M3 9h18v2a3 3 0 0 1-6 0 3 3 0 0 1-6 0 3 3 0 0 1-6 0z'/><path d='M5 13v8h14v-8'/><path d='M10 21v-5h4v5'/>",
        ["sair"] = "<path d='M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4'/><polyline points='16 17 21 12 16 7'/><line x1='21' y1='12' x2='9' y2='12'/>",
        ["menu"] = "<line x1='3' y1='12' x2='21' y2='12'/><line x1='3' y1='6' x2='21' y2='6'/><line x1='3' y1='18' x2='21' y2='18'/>",
        ["fechar"] = "<line x1='18' y1='6' x2='6' y2='18'/><line x1='6' y1='6' x2='18' y2='18'/>",
        ["mais"] = "<line x1='12' y1='5' x2='12' y2='19'/><line x1='5' y1='12' x2='19' y2='12'/>",
        ["dinheiro"] = "<line x1='12' y1='1' x2='12' y2='23'/><path d='M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6'/>",
        ["carrinho"] = "<circle cx='9' cy='21' r='1'/><circle cx='20' cy='21' r='1'/><path d='M1 1h4l2.68 13.39a2 2 0 0 0 2 1.61h9.72a2 2 0 0 0 2-1.61L23 6H6'/>",
        ["alerta"] = "<path d='M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z'/><line x1='12' y1='9' x2='12' y2='13'/><line x1='12' y1='17' x2='12.01' y2='17'/>",
        ["estoque"] = "<polyline points='21 8 21 21 3 21 3 8'/><rect x='1' y='3' width='22' height='5'/><line x1='10' y1='12' x2='14' y2='12'/>",
        ["entrada"] = "<circle cx='12' cy='12' r='10'/><polyline points='8 12 12 16 16 12'/><line x1='12' y1='8' x2='12' y2='16'/>",
        ["saida"] = "<circle cx='12' cy='12' r='10'/><polyline points='16 12 12 8 8 12'/><line x1='12' y1='16' x2='12' y2='8'/>",
        ["busca"] = "<circle cx='11' cy='11' r='8'/><line x1='21' y1='21' x2='16.65' y2='16.65'/>",
        ["editar"] = "<path d='M12 20h9'/><path d='M16.5 3.5a2.121 2.121 0 0 1 3 3L7 19l-4 1 1-4L16.5 3.5z'/>",
        ["excluir"] = "<polyline points='3 6 5 6 21 6'/><path d='M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2'/>",
        ["seta-direita"] = "<line x1='5' y1='12' x2='19' y2='12'/><polyline points='12 5 19 12 12 19'/>",
        ["calendario"] = "<rect x='3' y='4' width='18' height='18' rx='2' ry='2'/><line x1='16' y1='2' x2='16' y2='6'/><line x1='8' y1='2' x2='8' y2='6'/><line x1='3' y1='10' x2='21' y2='10'/>",
        ["grafico"] = "<line x1='18' y1='20' x2='18' y2='10'/><line x1='12' y1='20' x2='12' y2='4'/><line x1='6' y1='20' x2='6' y2='14'/>",
        ["cadeado"] = "<rect x='3' y='11' width='18' height='11' rx='2' ry='2'/><path d='M7 11V7a5 5 0 0 1 10 0v4'/>",
        ["ok"] = "<path d='M22 11.08V12a10 10 0 1 1-5.93-9.14'/><polyline points='22 4 12 14.01 9 11.01'/>",
        ["historico"] = "<circle cx='12' cy='12' r='10'/><polyline points='12 6 12 12 16 14'/>",
        ["info"] = "<circle cx='12' cy='12' r='10'/><line x1='12' y1='16' x2='12' y2='12'/><line x1='12' y1='8' x2='12.01' y2='8'/>",
        ["filtro"] ="<polygon points='22 3 2 3 10 12.46 10 19 14 21 14 12.46 22 3'/>"
    };

    /// <summary>Devolve o &lt;svg&gt; do ícone pedido (ou vazio, se o nome não existir).</summary>
    public static IHtmlContent Svg(string nome, string classe = "icone")
    {
        if (!Desenhos.TryGetValue(nome, out var desenho))
            return HtmlString.Empty;

        return new HtmlString(
            $"<svg class='{classe}' viewBox='0 0 24 24' fill='none' stroke='currentColor' stroke-width='2' " +
            $"stroke-linecap='round' stroke-linejoin='round' aria-hidden='true' focusable='false'>{desenho}</svg>");
    }
}
