using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockControl.Models;

namespace StockControl.Data.Configurations;

/// <summary>Mapeamento (Fluent API) da classe Usuario para a tabela Usuario.</summary>
public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuario");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Nome).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(150);
        builder.Property(u => u.SenhaHash).IsRequired().HasMaxLength(100);

        // O enum é gravado como texto: PerfilUsuario.Admin <-> 'Admin'
        builder.Property(u => u.Perfil).IsRequired().HasMaxLength(20).HasConversion<string>();

        builder.Property(u => u.Ativo).IsRequired();
        builder.Property(u => u.CriadoEm).IsRequired();
        builder.Property(u => u.AtualizadoEm).IsRequired();

        // Documenta o índice único que o script SQL já criou.
        builder.HasIndex(u => u.Email).IsUnique().HasDatabaseName("UX_Usuario_Email");
    }
}
