using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockControl.Models;

namespace StockControl.Data.Configurations;

/// <summary>Mapeamento (Fluent API) da classe LogAuditoria para a tabela LogAuditoria.</summary>
public class LogAuditoriaConfiguration : IEntityTypeConfiguration<LogAuditoria>
{
    public void Configure(EntityTypeBuilder<LogAuditoria> builder)
    {
        builder.ToTable("LogAuditoria");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Tabela).IsRequired().HasMaxLength(50);
        builder.Property(l => l.Acao).IsRequired().HasMaxLength(20);
        builder.Property(l => l.RegistroId).IsRequired();
        builder.Property(l => l.ValorAntigo).HasMaxLength(500);
        builder.Property(l => l.ValorNovo).HasMaxLength(500);
        builder.Property(l => l.Data).IsRequired();
    }
}
