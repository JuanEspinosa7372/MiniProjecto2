using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Data;

/// <summary>Permite crear migraciones (dotnet ef) sin depender de la configuración de la web.</summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var opciones = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(RutaBaseDatos.CadenaConexion)
            .Options;
        return new AppDbContext(opciones);
    }
}
