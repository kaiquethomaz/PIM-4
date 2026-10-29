using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StockControl.Data;
using StockControl.Excecoes;
using StockControl.Models;
using StockControl.Models.Consultas;

namespace StockControl.DAO;

public class MovimentacaoDAO : IMovimentacaoDAO
{
    private readonly AppDbContext _contexto;

    public MovimentacaoDAO(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<int> RegistrarAsync(Movimentacao movimentacao)
    {
        var tipo = movimentacao.Tipo == TipoMovimentacao.Entrada ? "ENTRADA" : "SAIDA";

        try
        {
            // String interpolada ($"...{valor}..."): o EF transforma cada {valor}
            // em um parâmetro SQL (@p0, @p1...). Isso evita SQL Injection.
            // O INSERT é feito pela procedure; o estoque é atualizado pela trigger.
            // A procedure só aceita produto e usuário do estabelecimento informado.
            var resultado = await _contexto.Database
                .SqlQuery<IdGerado>($@"EXEC sp_RegistrarMovimentacao
                    @EstabelecimentoId = {_contexto.EstabelecimentoAtualId},
                    @ProdutoId  = {movimentacao.ProdutoId},
                    @Tipo       = {tipo},
                    @Quantidade = {movimentacao.Quantidade},
                    @UsuarioId  = {movimentacao.UsuarioId},
                    @Observacao = {movimentacao.Observacao}")
                .ToListAsync();

            movimentacao.Id = resultado.Single().Id;

            // A trigger mudou o EstoqueAtual direto no banco, "por fora" do EF.
            // Se este produto já estava carregado na memória nesta requisição,
            // recarregamos do banco para não devolver o estoque antigo.
            var produtoEmMemoria = _contexto.Produtos.Local.FirstOrDefault(p => p.Id == movimentacao.ProdutoId);
            if (produtoEmMemoria is not null)
                await _contexto.Entry(produtoEmMemoria).ReloadAsync();

            return movimentacao.Id;
        }
        catch (SqlException erro) when (erro.Number >= 50000)
        {
            // Erros 50001..50005 são os THROW da procedure (ex.: estoque insuficiente).
            // Viram RegraNegocioException para a tela mostrar a mensagem ao usuário.
            throw new RegraNegocioException(erro.Message);
        }
    }

    public async Task<List<MovimentacaoRelatorio>> ListarPorPeriodoAsync(DateTime dataInicio, DateTime dataFim)
    {
        try
        {
            return await _contexto.Database
                .SqlQuery<MovimentacaoRelatorio>(
                    $@"EXEC sp_RelatorioMovimentacoes
                        @EstabelecimentoId = {_contexto.EstabelecimentoAtualId},
                        @DataInicio = {dataInicio.Date},
                        @DataFim = {dataFim.Date}")
                .ToListAsync();
        }
        catch (SqlException erro) when (erro.Number >= 50000)
        {
            throw new RegraNegocioException(erro.Message);
        }
    }

    public async Task<List<Movimentacao>> ListarUltimasAsync(int quantidade)
    {
        return await _contexto.Movimentacoes.AsNoTracking()
            .Include(m => m.Produto)
            .Include(m => m.Usuario)
            .OrderByDescending(m => m.Data)
            .ThenByDescending(m => m.Id)
            .Take(quantidade)
            .ToListAsync();
    }

    public async Task<List<Movimentacao>> ListarVendasAsync(DateTime dataInicio, DateTime dataFim)
    {
        var inicio = dataInicio.Date;
        var fimExclusivo = dataFim.Date.AddDays(1);   // inclui o último dia inteiro

        return await _contexto.Movimentacoes.AsNoTracking()
            .Include(m => m.Produto!).ThenInclude(p => p.Categoria)
            .Include(m => m.Usuario)
            .Where(m => m.Tipo == TipoMovimentacao.Saida && m.Data >= inicio && m.Data < fimExclusivo)
            .OrderByDescending(m => m.Data)
            .ThenByDescending(m => m.Id)
            .ToListAsync();
    }

    public async Task<int> ContarDesdeAsync(TipoMovimentacao tipo, DateTime desde)
    {
        return await _contexto.Movimentacoes.CountAsync(m => m.Tipo == tipo && m.Data >= desde);
    }

    public async Task<List<VendaDia>> SomarVendasPorDiaAsync(DateTime desde)
    {
        // Vira um único SELECT ... GROUP BY CONVERT(date, Data) no banco.
        // O valor usa o preço atual do produto (a movimentação não guarda o preço da época).
        // O filtro automático garante que só entram as vendas do estabelecimento logado.
        return await _contexto.Movimentacoes.AsNoTracking()
            .Where(m => m.Tipo == TipoMovimentacao.Saida && m.Data >= desde)
            .Select(m => new { Dia = m.Data.Date, Valor = m.Quantidade * m.Produto!.Preco, m.Quantidade })
            .GroupBy(x => x.Dia)
            .Select(g => new VendaDia
            {
                Dia = g.Key,
                Valor = g.Sum(x => x.Valor),
                Itens = g.Sum(x => x.Quantidade)
            })
            .ToListAsync();
    }

    public async Task<List<ProdutoMaisVendido>> ListarMaisVendidosAsync(DateTime desde, int quantidade)
    {
        // SELECT TOP (n) ... GROUP BY ProdutoId, Nome ORDER BY SUM(Quantidade) DESC
        return await _contexto.Movimentacoes.AsNoTracking()
            .Where(m => m.Tipo == TipoMovimentacao.Saida && m.Data >= desde)
            .Select(m => new { m.ProdutoId, m.Produto!.Nome, m.Quantidade, Valor = m.Quantidade * m.Produto.Preco })
            .GroupBy(x => new { x.ProdutoId, x.Nome })
            .Select(g => new ProdutoMaisVendido
            {
                ProdutoId = g.Key.ProdutoId,
                Nome = g.Key.Nome,
                Quantidade = g.Sum(x => x.Quantidade),
                Valor = g.Sum(x => x.Valor)
            })
            .OrderByDescending(p => p.Quantidade)
            .Take(quantidade)
            .ToListAsync();
    }
}
