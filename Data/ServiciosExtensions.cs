using Core.Interfaces;
using Core.Servicios;
using Core.Tarifas;
using Data.Repositorios;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Data;

/// <summary>Método de extensión para registrar todo con una sola línea en Program.cs.</summary>
public static class ServiciosExtensions
{
    public static IServiceCollection AgregarHotelData(this IServiceCollection servicios)
    {
        servicios.AddDbContext<AppDbContext>(o => o.UseSqlite(RutaBaseDatos.CadenaConexion));
        servicios.AddScoped(typeof(IRepositorio<>), typeof(Repositorio<>));

        // Varias implementaciones de ITarifa -> se inyectan como IEnumerable<ITarifa>
        servicios.AddScoped<ITarifa, TarifaTemporadaAlta>();
        servicios.AddScoped<ITarifa, TarifaLargaEstadia>();

        servicios.AddScoped<ReservaServicio>();
        return servicios;
    }
}
