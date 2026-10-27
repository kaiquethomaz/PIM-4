using Microsoft.EntityFrameworkCore;
using StockControl.Data.Configurations;
using StockControl.Models;
using StockControl.Services;

namespace StockControl.Data;

/// <summary>
/// "Ponte" entre o C# e o banco SQL Server.
/// IMPORTANTE: o banco é criado pelo script database/script_stockcontrol.sql.
/// O EF Core NÃO cria nem altera tabelas aqui (não usamos migrations):
/// ele apenas mapeia as tabelas que já existem.
/// </summary>
public class AppDbContext : DbContext
{
    private readonly IUsuarioAtual _usuarioAtual;

    public AppDbContext(DbContextOptions<AppDbContext> options, IUsuarioAtual usuarioAtual) : base(options)
    {
        _usuarioAtual = usuarioAtual;
    }

    /// <summary>
    /// Estabelecimento do usuário logado. É usado pelos filtros automáticos
    /// abaixo e pelos DAOs na chamada das procedures.
    /// </summary>
    public int? EstabelecimentoAtualId => _usuarioAtual.EstabelecimentoId;

    // Cada DbSet representa uma tabela do banco.
    public DbSet<Estabelecimento> Estabelecimentos => Set<Estabelecimento>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Fornecedor> Fornecedores => Set<Fornecedor>();
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<Movimentacao> Movimentacoes => Set<Movimentacao>();
    public DbSet<LogAuditoria> LogsAuditoria => Set<LogAuditoria>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Fluent API: cada entidade tem a sua classe de configuração
        // na pasta Data/Configurations (uma classe por tabela).
        modelBuilder.ApplyConfiguration(new EstabelecimentoConfiguration());
        modelBuilder.ApplyConfiguration(new UsuarioConfiguration());
        modelBuilder.ApplyConfiguration(new CategoriaConfiguration());
        modelBuilder.ApplyConfiguration(new FornecedorConfiguration());
        modelBuilder.ApplyConfiguration(new ProdutoConfiguration());
        modelBuilder.ApplyConfiguration(new MovimentacaoConfiguration());
        modelBuilder.ApplyConfiguration(new LogAuditoriaConfiguration());

        // -----------------------------------------------------------------
        // FILTROS AUTOMÁTICOS POR ESTABELECIMENTO (Global Query Filters)
        // O EF acrescenta estes WHERE em TODA consulta destas tabelas.
        // Exemplo: _contexto.Produtos.ToList() vira
        //   SELECT ... FROM Produto WHERE EstabelecimentoId = @estabelecimentoDoUsuario
        // Assim um mercadinho nunca enxerga os dados de outro, mesmo que
        // alguém esqueça o filtro em alguma tela. Sem ninguém logado
        // (EstabelecimentoAtualId = null), as consultas não trazem nada.
        // Para os raros casos em que é preciso ignorar o filtro (login,
        // e-mail único no sistema todo), o DAO usa IgnoreQueryFilters().
        // -----------------------------------------------------------------
        modelBuilder.Entity<Estabelecimento>().HasQueryFilter(e => e.Id == EstabelecimentoAtualId);
        modelBuilder.Entity<Usuario>().HasQueryFilter(u => u.EstabelecimentoId == EstabelecimentoAtualId);
        modelBuilder.Entity<Categoria>().HasQueryFilter(c => c.EstabelecimentoId == EstabelecimentoAtualId);
        modelBuilder.Entity<Fornecedor>().HasQueryFilter(f => f.EstabelecimentoId == EstabelecimentoAtualId);
        modelBuilder.Entity<Produto>().HasQueryFilter(p => p.EstabelecimentoId == EstabelecimentoAtualId);
        // Movimentação não tem EstabelecimentoId: o estabelecimento é o do produto.
        modelBuilder.Entity<Movimentacao>().HasQueryFilter(m => m.Produto!.EstabelecimentoId == EstabelecimentoAtualId);
        modelBuilder.Entity<LogAuditoria>().HasQueryFilter(l => l.EstabelecimentoId == EstabelecimentoAtualId);
    }

    /// <summary>
    /// Antes de salvar: todo registro NOVO que pertence a um estabelecimento
    /// (Usuario, Categoria, Fornecedor, Produto) e ainda está sem
    /// EstabelecimentoId recebe o estabelecimento do usuário logado.
    /// Assim nenhuma tela precisa se preocupar com isso.
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var novos = ChangeTracker.Entries<IPertenceAEstabelecimento>()
            .Where(e => e.State == EntityState.Added && e.Entity.EstabelecimentoId == 0);

        foreach (var entrada in novos)
        {
            entrada.Entity.EstabelecimentoId = EstabelecimentoAtualId
                ?? throw new InvalidOperationException("Não há estabelecimento definido para o registro.");
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
