using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockControl.Models;

namespace StockControl.Data.Configurations;

/// <summary>Mapeamento (Fluent API) da classe Fornecedor para a tabela Fornecedor.</summary>
public class FornecedorConfiguration : IEntityTypeConfiguration<Fornecedor>
{
    public void Configure(EntityTypeBuilder<Fornecedor> builder)
    {
        builder.ToTable("Fornecedor");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.RazaoSocial).IsRequired().HasMaxLength(150);

        // CHAR(14): tamanho fixo e sem acentos (IsUnicode(false))
        builder.Property(f => f.CNPJ).IsRequired().HasMaxLength(14).IsFixedLength().IsUnicode(false);

        builder.Property(f => f.Telefone).HasMaxLength(20);
        builder.Property(f => f.Email).HasMaxLength(150);
        builder.Property(f => f.AtualizadoEm).IsRequired();

        builder.HasOne<Estabelecimento>()
            .WithMany()
            .HasForeignKey(f => f.EstabelecimentoId)
            .OnDelete(DeleteBehavior.Restrict);

        // O mesmo fornecedor (CNPJ) pode atender vários estabelecimentos,
        // mas não pode ser cadastrado duas vezes no mesmo.
        builder.HasIndex(f => new { f.EstabelecimentoId, f.CNPJ }).IsUnique().HasDatabaseName("UX_Fornecedor_Estab_CNPJ");
    }
}
