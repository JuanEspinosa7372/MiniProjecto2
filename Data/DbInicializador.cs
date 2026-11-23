using Core.Constantes;
using Core.Entidades;
using Core.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Data;

/// <summary>Aplica migraciones y crea datos iniciales: roles, usuarios y habitaciones.</summary>
public static class DbInicializador
{
    public static async Task InicializarAsync(IServiceProvider proveedor)
    {
        using var alcance = proveedor.CreateScope();
        var servicios = alcance.ServiceProvider;

        var contexto = servicios.GetRequiredService<AppDbContext>();
        // Si ya existe una migración (dotnet ef migrations add Inicial) se aplica;
        // si no, se crea la base de datos directamente desde el modelo.
        if (contexto.Database.GetMigrations().Any())
            await contexto.Database.MigrateAsync();
        else
            await contexto.Database.EnsureCreatedAsync();

        // Roles
        var roleManager = servicios.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var rol in Roles.Todos)
        {
            if (!await roleManager.RoleExistsAsync(rol))
                await roleManager.CreateAsync(new IdentityRole(rol));
        }

        // Usuarios de prueba (cambiar las claves antes de la entrega final)
        var userManager = servicios.GetRequiredService<UserManager<IdentityUser>>();
        await CrearUsuarioAsync(userManager, "admin@hotel.com", "Admin123*", Roles.Admin);
        await CrearUsuarioAsync(userManager, "recepcion@hotel.com", "Recepcion123*", Roles.Recepcionista);

        // Habitaciones iniciales
        if (!await contexto.Habitaciones.AnyAsync())
        {
            contexto.Habitaciones.AddRange(
                new Habitacion { Numero = "101", Tipo = TipoHabitacion.Sencilla, Capacidad = 1, PrecioNoche = 120000 },
                new Habitacion { Numero = "102", Tipo = TipoHabitacion.Sencilla, Capacidad = 1, PrecioNoche = 120000 },
                new Habitacion { Numero = "103", Tipo = TipoHabitacion.Sencilla, Capacidad = 2, PrecioNoche = 140000 },
                new Habitacion { Numero = "201", Tipo = TipoHabitacion.Doble, Capacidad = 2, PrecioNoche = 180000 },
                new Habitacion { Numero = "202", Tipo = TipoHabitacion.Doble, Capacidad = 3, PrecioNoche = 210000 },
                new Habitacion { Numero = "203", Tipo = TipoHabitacion.Doble, Capacidad = 4, PrecioNoche = 240000 },
                new Habitacion { Numero = "301", Tipo = TipoHabitacion.Suite, Capacidad = 4, PrecioNoche = 350000 });
            await contexto.SaveChangesAsync();
        }
    }

    private static async Task CrearUsuarioAsync(
        UserManager<IdentityUser> userManager, string email, string clave, string rol)
    {
        var usuario = await userManager.FindByEmailAsync(email);
        if (usuario is null)
        {
            usuario = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
            var resultado = await userManager.CreateAsync(usuario, clave);
            if (!resultado.Succeeded) return;
        }

        if (!await userManager.IsInRoleAsync(usuario, rol))
            await userManager.AddToRoleAsync(usuario, rol);
    }
}
