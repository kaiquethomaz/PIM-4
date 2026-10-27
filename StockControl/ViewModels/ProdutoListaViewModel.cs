using StockControl.Models;

namespace StockControl.ViewModels;

/// <summary>Dados da tela de listagem de produtos (com busca).</summary>
public class ProdutoListaViewModel
{
    public string? Busca { get; set; }
    public bool MostrarInativos { get; set; }
    public bool PodeGerenciar { get; set; }   // Admin/Gerente veem os botões de editar e excluir
    public List<Produto> Produtos { get; set; } = new();
}
