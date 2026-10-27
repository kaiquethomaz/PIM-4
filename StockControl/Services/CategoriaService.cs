using StockControl.DAO;
using StockControl.Excecoes;
using StockControl.Models;

namespace StockControl.Services;

public class CategoriaService : ICategoriaService
{
    // O Service conversa com o banco só por meio do DAO (nunca direto no DbContext).
    private readonly ICategoriaDAO _categoriaDAO;

    public CategoriaService(ICategoriaDAO categoriaDAO)
    {
        _categoriaDAO = categoriaDAO;
    }

    public Task<List<Categoria>> ListarAsync() => _categoriaDAO.ListarTodosAsync();

    public Task<Categoria?> ObterAsync(int id) => _categoriaDAO.BuscarPorIdAsync(id);

    public async Task<Categoria> CriarAsync(Categoria categoria)
    {
        await ValidarAsync(categoria);
        await _categoriaDAO.InserirAsync(categoria);
        return categoria;
    }

    public async Task AtualizarAsync(Categoria categoria)
    {
        var existente = await _categoriaDAO.BuscarPorIdAsync(categoria.Id)
            ?? throw new RegraNegocioException("Categoria não encontrada.");

        await ValidarAsync(categoria);

        existente.Nome = categoria.Nome;
        existente.Descricao = categoria.Descricao;
        await _categoriaDAO.AtualizarAsync(existente);
    }

    public async Task ExcluirAsync(int id)
    {
        var categoria = await _categoriaDAO.BuscarPorIdAsync(id)
            ?? throw new RegraNegocioException("Categoria não encontrada.");

        if (await _categoriaDAO.PossuiProdutosAsync(id))
            throw new RegraNegocioException("Não é possível excluir: existem produtos nesta categoria.");

        await _categoriaDAO.ExcluirAsync(categoria);
    }

    private async Task ValidarAsync(Categoria categoria)
    {
        categoria.Nome = (categoria.Nome ?? string.Empty).Trim();
        categoria.Descricao = string.IsNullOrWhiteSpace(categoria.Descricao) ? null : categoria.Descricao.Trim();

        if (categoria.Nome.Length == 0)
            throw new RegraNegocioException("Informe o nome da categoria.");

        if (await _categoriaDAO.NomeExisteAsync(categoria.Nome, categoria.Id))
            throw new RegraNegocioException("Já existe uma categoria com este nome.");
    }
}
