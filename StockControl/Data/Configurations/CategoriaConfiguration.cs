using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockControl.Models;

namespace StockControl.Data.Configurations;

/// <summary>Mapeamento (Fluent API) da classe Categoria para a tabela Categoria.</summary>
public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("Categoria");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nome).IsRequired().HasMaxLength(60);
        builder.Property(c => c.Descricao).HasMaxLength(250);
        builder.Property(c => c.AtualizadoEm).IsRequired();

        // Cada categoria pertence a um estabelecimento (FK sem propriedade de navegação).
        builder.HasOne<Estabelecimento>()
            .WithMany()
            .HasForeignKey(c => c.EstabelecimentoId)
            .OnDelete(DeleteBehavior.Restrict);

        // O nome se repete entre estabelecimentos, mas não dentro do mesmo.
        builder.HasIndex(c => new { c.EstabelecimentoId, c.Nome }).IsUnique().HasDatabaseName("UX_Categoria_Estab_Nome");
    }
}
