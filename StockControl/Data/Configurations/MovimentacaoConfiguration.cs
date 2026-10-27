using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockControl.Models;

namespace StockControl.Data.Configurations;

/// <summary>Mapeamento (Fluent API) da classe Movimentacao para a tabela Movimentacao.</summary>
public class MovimentacaoConfiguration : IEntityTypeConfiguration<Movimentacao>
{
    public void Configure(EntityTypeBuilder<Movimentacao> builder)
    {
        // A tabela tem a trigger trg_AtualizaEstoque (AFTER INSERT): avisamos o EF.
        builder.ToTable("Movimentacao", tb => tb.HasTrigger("trg_AtualizaEstoque"));
        builder.HasKey(m => m.Id);

        // O enum vira 'ENTRADA' / 'SAIDA' no banco (e volta para o enum ao ler).
        builder.Property(m => m.Tipo)
            .IsRequired()
            .HasMaxLength(7)
            .IsUnicode(false)
            .HasConversion(
                tipo => tipo == TipoMovimentacao.Entrada ? "ENTRADA" : "SAIDA",
                texto => texto == "ENTRADA" ? TipoMovimentacao.Entrada : TipoMovimentacao.Saida);

        builder.Property(m => m.Quantidade).IsRequired();
        builder.Property(m => m.Data).IsRequired();
        builder.Property(m => m.Observacao).HasMaxLength(250);

        builder.HasOne(m => m.Produto)
            .WithMany(p => p.Movimentacoes)
            .HasForeignKey(m => m.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Usuario)
            .WithMany()
            .HasForeignKey(m => m.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => m.Data).HasDatabaseName("IX_Movimentacao_Data");
    }
}
