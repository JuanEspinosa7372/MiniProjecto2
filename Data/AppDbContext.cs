using Core.Entidades;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Data;

/// <summary>Contexto de EF Core: tablas del hotel + tablas de Identity.</summary>
public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Habitacion> Habitaciones => Set<Habitacion>();
    public DbSet<Huesped> Huespedes => Set<Huesped>();
    public DbSet<Reserva> Reservas => Set<Reserva>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // necesario para las tablas de Identity

        builder.Entity<Habitacion>(e =>
        {
            e.HasIndex(h => h.Numero).IsUnique();
            e.Property(h => h.PrecioNoche).HasPrecision(18, 2);
            e.Property(h => h.Tipo).HasConversion<string>();
            e.Property(h => h.Estado).HasConversion<string>();
        });

        builder.Entity<Huesped>(e =>
        {
            e.HasIndex(h => h.Documento).IsUnique();
        });

        builder.Entity<Reserva>(e =>
        {
            e.Property(r => r.Total).HasPrecision(18, 2);
            e.Property(r => r.Estado).HasConversion<string>();

            e.HasOne(r => r.Huesped)
             .WithMany(h => h.Reservas)
             .HasForeignKey(r => r.HuespedId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(r => r.Habitacion)
             .WithMany(h => h.Reservas)
             .HasForeignKey(r => r.HabitacionId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
