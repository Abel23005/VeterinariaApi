using Microsoft.EntityFrameworkCore;
using Veterinaria.Entidades.Models;

namespace Veterinaria.Datos.Data;

// Modelo equivalente al generado por Scaffold desde sql/01_create_database.sql.
public partial class VeterinariaContext(DbContextOptions<VeterinariaContext> options) : DbContext(options)
{
    public virtual DbSet<Dueno> Duenos => Set<Dueno>();
    public virtual DbSet<Mascota> Mascotas => Set<Mascota>();
    public virtual DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Dueno>(entity =>
        {
            entity.HasKey(e => e.DuenoId);
            entity.ToTable("Duenos");
            entity.Property(e => e.DuenoId).HasColumnName("DuenoID");
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.Correo).HasMaxLength(150);
            entity.Property(e => e.Telefono).HasMaxLength(20);
            entity.HasIndex(e => e.Correo).IsUnique();
        });
        modelBuilder.Entity<Usuario>(entity => { entity.HasKey(e=>e.UsuarioId); entity.ToTable("Usuarios"); entity.Property(e=>e.UsuarioId).HasColumnName("UsuarioID"); entity.Property(e=>e.Correo).HasMaxLength(150).IsRequired(); entity.Property(e=>e.ClaveHash).HasMaxLength(200).IsRequired(); entity.Property(e=>e.Rol).HasMaxLength(40).IsRequired(); entity.HasIndex(e=>e.Correo).IsUnique(); });
        modelBuilder.Entity<Mascota>(entity =>
        {
            entity.HasKey(e => e.MascotaId);
            entity.ToTable("Mascotas");
            entity.Property(e => e.MascotaId).HasColumnName("MascotaID");
            entity.Property(e => e.Nombre).HasMaxLength(80);
            entity.Property(e => e.Especie).HasMaxLength(40);
            entity.Property(e => e.Raza).HasMaxLength(60);
            entity.Property(e => e.DuenoId).HasColumnName("DuenoID");
            entity.HasOne(e => e.Dueno).WithMany(e => e.Mascotas).HasForeignKey(e => e.DuenoId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
