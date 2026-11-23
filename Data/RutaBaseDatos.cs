namespace Data;

/// <summary>
/// Ruta única del archivo SQLite: la web y el escritorio usan la misma base de datos.
/// </summary>
public static class RutaBaseDatos
{
    public static string Archivo
    {
        get
        {
            var carpeta = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HotelApp");
            Directory.CreateDirectory(carpeta);
            return Path.Combine(carpeta, "hotel.db");
        }
    }

    public static string CadenaConexion => $"Data Source={Archivo}";
}
