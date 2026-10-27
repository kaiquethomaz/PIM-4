using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockControl.Models;

namespace StockControl.Data.Configurations;

/// <summary>Mapeamento (Fluent API) da classe Produto para a tabela Produto.</summary>
public class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        // A tabela Produto tem a trigger trg_AuditoriaProduto.
        // Desde o EF Core 7, se o EF não souber da trigger, o UPDATE/DELETE dá erro.
        builder.ToTable("Produto", tb => tb.HasTrigger("trg_AuditoriaProduto"));
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nome).IsRequired().HasMaxLength(120);
        builder.Property(p => p.CodigoBarras).IsRequired().HasMaxLength(20).IsUnicode(false);
        builder.Property(p => p.Preco).IsRequired().HasPrecision(10, 2);
        builder.Property(p => p.EstoqueMinimo).IsRequired();
        builder.Property(p => p.Ativo).IsRequired();
        builder.Property(p => p.AtualizadoEm).IsRequired();

        // EstoqueAtual é responsabilidade da trigger trg_AtualizaEstoque.
        // "Ignore" depois de salvo = o EF nunca envia esse campo num UPDATE,
        // então não corre o risco de sobrescrever o valor calculado pelo banco.
        builder.Property(p => p.EstoqueAtual).IsRequired()
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

        // Relacionamentos 1:N. "Restrict" = não deixa apagar uma categoria
        // ou fornecedor que ainda tenha produtos (igual à FK do script).
        builder.HasOne(p => p.Categoria)
            .WithMany(c => c.Produtos)
            .HasForeignKey(p => p.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Fornecedor)
            .WithMany(f => f.Produtos)
            .HasForeignKey(p => p.FornecedorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Estabelecimento>()
            .WithMany()
            .HasForeignKey(p => p.EstabelecimentoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Código de barras único dentro de cada estabelecimento
        // (dois mercadinhos podem vender o mesmo arroz, com o mesmo código).
        builder.HasIndex(p => new { p.EstabelecimentoId, p.CodigoBarras }).IsUnique().HasDatabaseName("UX_Produto_Estab_CodigoBarras");
    }
}
