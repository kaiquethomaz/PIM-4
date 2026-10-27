using Microsoft.EntityFrameworkCore;
using StockControl.Data.Configurations;
using StockControl.Models;

namespace StockControl.Data;

/// <summary>
/// "Ponte" entre o C# e o banco SQL Server.
/// IMPORTANTE: o banco é criado pelo script database/script_stockcontrol.sql.
/// O EF Core NÃO cria nem altera tabelas aqui (não usamos migrations):
/// ele apenas mapeia as tabelas que já existem.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Cada DbSet representa uma tabela do banco.
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
        modelBuilder.ApplyConfiguration(new UsuarioConfiguration());
        modelBuilder.ApplyConfiguration(new CategoriaConfiguration());
        modelBuilder.ApplyConfiguration(new FornecedorConfiguration());
        modelBuilder.ApplyConfiguration(new ProdutoConfiguration());
        modelBuilder.ApplyConfiguration(new MovimentacaoConfiguration());
        modelBuilder.ApplyConfiguration(new LogAuditoriaConfiguration());
    }
}
