using DesafioInventario.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DesafioInventario.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Produto> Produtos => Set<Produto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.ToTable("categorias");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).HasColumnName("id");
            entity.Property(c => c.Nome)
                .HasColumnName("nome")
                .IsRequired()
                .HasMaxLength(120);
            entity.Property(c => c.Descricao)
                .HasColumnName("descricao")
                .HasMaxLength(500);

            entity.HasIndex(c => c.Nome).IsUnique();
        });

        modelBuilder.Entity<Produto>(entity =>
        {
            entity.ToTable("produtos");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).HasColumnName("id");
            entity.Property(p => p.Nome)
                .HasColumnName("nome")
                .IsRequired()
                .HasMaxLength(160);
            entity.Property(p => p.Descricao)
                .HasColumnName("descricao")
                .HasMaxLength(1000);
            entity.Property(p => p.Preco)
                .HasColumnName("preco")
                .HasColumnType("numeric(12,2)")
                .IsRequired();
            entity.Property(p => p.CategoriaId).HasColumnName("categoria_id");

            entity.HasOne(p => p.Categoria)
                .WithMany(c => c.Produtos)
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(p => p.CategoriaId);
        });
    }
}
