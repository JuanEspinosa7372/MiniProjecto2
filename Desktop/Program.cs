using Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Desktop;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        // Misma configuración de servicios que la web: mismos repositorios, servicios y base de datos.
        var servicios = new ServiceCollection();
        servicios.AddLogging();
        servicios.AgregarHotelData();
        servicios.AddIdentityCore<IdentityUser>()
                 .AddRoles<IdentityRole>()
                 .AddEntityFrameworkStores<AppDbContext>();

        using var proveedor = servicios.BuildServiceProvider();

        // Aplica migraciones y crea roles, usuarios y habitaciones iniciales (si no existen).
        DbInicializador.InicializarAsync(proveedor).GetAwaiter().GetResult();

        ApplicationConfiguration.Initialize();

        using var login = new LoginForm(proveedor);
        if (login.ShowDialog() != DialogResult.OK) return;

        Application.Run(new MainForm(proveedor, login.EsAdmin, login.Usuario));
    }
}
