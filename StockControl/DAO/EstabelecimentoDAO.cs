using Microsoft.EntityFrameworkCore;
using StockControl.Data;
using StockControl.Models;

namespace StockControl.DAO;

public class EstabelecimentoDAO : DAOBase<Estabelecimento>, IEstabelecimentoDAO
{
    public EstabelecimentoDAO(AppDbContext contexto) : base(contexto)
    {
    }

    public async Task<bool> CnpjExisteAsync(string cnpj)
    {
        // Cadastro feito por alguém que ainda não está logado:
        // ignora o filtro automático e procura em todos os estabelecimentos.
        return await _tabela.IgnoreQueryFilters().AnyAsync(e => e.CNPJ == cnpj);
    }

    public async Task CadastrarComAdministradorAsync(
        Estabelecimento estabelecimento, Usuario administrador, IEnumerable<Categoria> categoriasIniciais)
    {
        // Transação: se qualquer passo falhar (ex.: e-mail repetido no banco),
        // o ROLLBACK desfaz tudo e não fica um estabelecimento "sem dono".
        await using var transacao = await _contexto.Database.BeginTransactionAsync();

        // 1. Estabelecimento (o banco gera o Id)
        estabelecimento.MarcarComoAtualizado();
        _tabela.Add(estabelecimento);
        await _contexto.SaveChangesAsync();

        // 2. Dono (Admin) e categorias iniciais, já ligados ao novo estabelecimento
        administrador.EstabelecimentoId = estabelecimento.Id;
        administrador.MarcarComoAtualizado();
        _contexto.Usuarios.Add(administrador);

        foreach (var categoria in categoriasIniciais)
        {
            categoria.EstabelecimentoId = estabelecimento.Id;
            categoria.MarcarComoAtualizado();
            _contexto.Categorias.Add(categoria);
        }

        await _contexto.SaveChangesAsync();
        await transacao.CommitAsync();
    }
}
