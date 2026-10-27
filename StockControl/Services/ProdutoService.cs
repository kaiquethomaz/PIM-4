using StockControl.DAO;
using StockControl.Excecoes;
using StockControl.Models;

namespace StockControl.Services;

public class ProdutoService : IProdutoService
{
    private readonly IProdutoDAO _produtoDAO;
    private readonly ICategoriaDAO _categoriaDAO;
    private readonly IFornecedorDAO _fornecedorDAO;
    private readonly IMovimentacaoDAO _movimentacaoDAO;

    // Injeção de dependência: o ASP.NET cria os DAOs e entrega prontos aqui
    // (o registro está no Program.cs).
    public ProdutoService(
        IProdutoDAO produtoDAO,
        ICategoriaDAO categoriaDAO,
        IFornecedorDAO fornecedorDAO,
        IMovimentacaoDAO movimentacaoDAO)
    {
        _produtoDAO = produtoDAO;
        _categoriaDAO = categoriaDAO;
        _fornecedorDAO = fornecedorDAO;
        _movimentacaoDAO = movimentacaoDAO;
    }

    public Task<List<Produto>> ListarAsync(string? busca = null, bool somenteAtivos = true)
        => _produtoDAO.BuscarAsync(busca, somenteAtivos);

    public Task<Produto?> ObterAsync(int id) => _produtoDAO.BuscarPorIdAsync(id);

    public Task<Produto?> ObterPorCodigoBarrasAsync(string codigoBarras)
        => _produtoDAO.BuscarPorCodigoBarrasAsync(codigoBarras.Trim());

    public Task<List<Produto>> ListarAtualizadosDesdeAsync(DateTime desde)
        => _produtoDAO.ListarAtualizadosDesdeAsync(desde);

    public async Task<Produto> CriarAsync(Produto produto, int estoqueInicial, int usuarioId)
    {
        if (estoqueInicial < 0)
            throw new RegraNegocioException("O estoque inicial não pode ser negativo.");

        await ValidarAsync(produto);
        await _produtoDAO.InserirAsync(produto);

        if (estoqueInicial > 0)
        {
            await _movimentacaoDAO.RegistrarAsync(new Movimentacao
            {
                ProdutoId = produto.Id,
                Tipo = TipoMovimentacao.Entrada,
                Quantidade = estoqueInicial,
                UsuarioId = usuarioId,
                Observacao = "Estoque inicial (cadastro do produto)"
            });
        }

        return produto;
    }

    public async Task AtualizarAsync(Produto produto)
    {
        var existente = await _produtoDAO.BuscarPorIdAsync(produto.Id)
            ?? throw new RegraNegocioException("Produto não encontrado.");

        await ValidarAsync(produto);

        // Copia só os campos editáveis. EstoqueAtual NÃO é copiado:
        // estoque só muda por movimentação.
        existente.Nome = produto.Nome;
        existente.CodigoBarras = produto.CodigoBarras;
        existente.Preco = produto.Preco;
        existente.EstoqueMinimo = produto.EstoqueMinimo;
        existente.CategoriaId = produto.CategoriaId;
        existente.FornecedorId = produto.FornecedorId;
        existente.Ativo = produto.Ativo;

        await _produtoDAO.AtualizarAsync(existente);
        // Se o preço mudou, a trigger trg_AuditoriaProduto grava em LogAuditoria.
    }

    public async Task<string> ExcluirAsync(int id)
    {
        var produto = await _produtoDAO.BuscarPorIdAsync(id)
            ?? throw new RegraNegocioException("Produto não encontrado.");

        if (await _produtoDAO.PossuiMovimentacoesAsync(id))
        {
            // Exclusão lógica: o produto some das telas, mas o histórico continua.
            produto.Ativo = false;
            await _produtoDAO.AtualizarAsync(produto);
            return $"O produto \"{produto.Nome}\" tem movimentações registradas, por isso foi desativado (não excluído).";
        }

        await _produtoDAO.ExcluirAsync(produto);
        // A trigger trg_AuditoriaProduto grava a exclusão em LogAuditoria.
        return $"Produto \"{produto.Nome}\" excluído.";
    }

    private async Task ValidarAsync(Produto produto)
    {
        produto.Nome = (produto.Nome ?? string.Empty).Trim();
        produto.CodigoBarras = (produto.CodigoBarras ?? string.Empty).Trim();

        if (produto.Nome.Length == 0)
            throw new RegraNegocioException("Informe o nome do produto.");

        if (produto.CodigoBarras.Length == 0)
            throw new RegraNegocioException("Informe o código de barras.");

        if (produto.Preco < 0)
            throw new RegraNegocioException("O preço não pode ser negativo.");

        if (produto.EstoqueMinimo < 0)
            throw new RegraNegocioException("O estoque mínimo não pode ser negativo.");

        if (await _categoriaDAO.BuscarPorIdAsync(produto.CategoriaId) is null)
            throw new RegraNegocioException("Categoria inválida.");

        if (await _fornecedorDAO.BuscarPorIdAsync(produto.FornecedorId) is null)
            throw new RegraNegocioException("Fornecedor inválido.");

        if (await _produtoDAO.CodigoBarrasExisteAsync(produto.CodigoBarras, produto.Id))
            throw new RegraNegocioException("Já existe um produto com este código de barras.");
    }
}
