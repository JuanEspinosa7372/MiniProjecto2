namespace Core.Constantes;

/// <summary>Nombres de los roles del sistema (se usan en Identity y en [Authorize]).</summary>
public static class Roles
{
    public const string Admin = "Admin";
    public const string Recepcionista = "Recepcionista";
    public const string Huesped = "Huesped";

    // Para [Authorize(Roles = Roles.Personal)] -> Admin o Recepcionista
    public const string Personal = Admin + "," + Recepcionista;

    public static readonly string[] Todos = { Admin, Recepcionista, Huesped };
}
