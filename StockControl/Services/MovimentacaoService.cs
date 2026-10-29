using StockControl.DAO;
using StockControl.Excecoes;
using StockControl.Models;
using StockControl.Models.Consultas;

namespace StockControl.Services;

public class MovimentacaoService : IMovimentacaoService
{
    private readonly IMovimentacaoDAO _movimentacaoDAO;
    private readonly IProdutoDAO _produtoDAO;

    public MovimentacaoService(IMovimentacaoDAO movimentacaoDAO, IProdutoDAO produtoDAO)
    {
        _movimentacaoDAO = movimentacaoDAO;
        _produtoDAO = produtoDAO;
    }

    public async Task<Movimentacao> RegistrarAsync(
        int produtoId, TipoMovimentacao tipo, int quantidade, int usuarioId, string? observacao)
    {
        if (quantidade <= 0)
            throw new RegraNegocioException("A quantidade deve ser maior que zero.");

        var produto = await _produtoDAO.BuscarPorIdAsync(produtoId);
        if (produto is null || !produto.Ativo)
            throw new RegraNegocioException("Produto não encontrado ou inativo.");

        // Regra principal: não vender mais do que existe no estoque.
        // (A procedure confere de novo no banco, com a linha travada, para o
        // caso de duas vendas chegarem ao mesmo tempo.)
        if (tipo == TipoMovimentacao.Saida && !produto.TemEstoquePara(quantidade))
            throw new RegraNegocioException(
                $"Estoque insuficiente. \"{produto.Nome}\" tem {produto.EstoqueAtual} unidade(s).");

        var movimentacao = new Movimentacao
        {
            ProdutoId = produtoId,
            Tipo = tipo,
            Quantidade = quantidade,
            UsuarioId = usuarioId,
            Observacao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim()
        };

        await _movimentacaoDAO.RegistrarAsync(movimentacao);
        return movimentacao;
    }

    public async Task<List<MovimentacaoRelatorio>> ListarPorPeriodoAsync(DateTime dataInicio, DateTime dataFim)
    {
        if (dataInicio.Date > dataFim.Date)
            throw new RegraNegocioException("A data inicial não pode ser maior que a data final.");

        return await _movimentacaoDAO.ListarPorPeriodoAsync(dataInicio, dataFim);
    }

    public Task<List<Movimentacao>> ListarUltimasAsync(int quantidade = 10)
        => _movimentacaoDAO.ListarUltimasAsync(quantidade);

    public async Task<List<Movimentacao>> ListarVendasAsync(DateTime dataInicio, DateTime dataFim)
    {
        if (dataInicio.Date > dataFim.Date)
            throw new RegraNegocioException("A data inicial não pode ser maior que a data final.");

        return await _movimentacaoDAO.ListarVendasAsync(dataInicio, dataFim);
    }
}
