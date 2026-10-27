using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockControl.Models;

namespace StockControl.Data.Configurations;

/// <summary>Mapeamento (Fluent API) da classe Estabelecimento para a tabela Estabelecimento.</summary>
public class EstabelecimentoConfiguration : IEntityTypeConfiguration<Estabelecimento>
{
    public void Configure(EntityTypeBuilder<Estabelecimento> builder)
    {
        builder.ToTable("Estabelecimento");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.NomeFantasia).IsRequired().HasMaxLength(120);
        builder.Property(e => e.CNPJ).IsRequired().HasMaxLength(14).IsFixedLength().IsUnicode(false);
        builder.Property(e => e.Telefone).HasMaxLength(20);
        builder.Property(e => e.Email).HasMaxLength(150);
        builder.Property(e => e.Ativo).IsRequired();
        builder.Property(e => e.CriadoEm).IsRequired();
        builder.Property(e => e.AtualizadoEm).IsRequired();

        builder.HasIndex(e => e.CNPJ).IsUnique().HasDatabaseName("UX_Estabelecimento_CNPJ");
    }
}
