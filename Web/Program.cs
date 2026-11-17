using System.Globalization;
using Data;
using Microsoft.AspNetCore.Identity;

// Cultura invariante: evita problemas al enviar números decimales desde los formularios.
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);

// Base de datos (EF Core + SQLite), repositorios y servicios de negocio
builder.Services.AgregarHotelData();

// ASP.NET Core Identity con roles
builder.Services.AddIdentity<IdentityUser, IdentityRole>(opciones =>
{
    opciones.Password.RequiredLength = 6;
    opciones.Password.RequireNonAlphanumeric = false;
    opciones.Password.RequireUppercase = false;
    opciones.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(opciones =>
{
    opciones.LoginPath = "/Account/Login";
    opciones.AccessDeniedPath = "/Account/AccesoDenegado";
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Migraciones / creación de la base de datos, roles, usuarios y habitaciones iniciales
await DbInicializador.InicializarAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
