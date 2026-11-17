using System.Globalization;

namespace Web.Helpers;

/// <summary>Métodos de extensión para mostrar valores con formato colombiano.</summary>
public static class FormatoExtensions
{
    private static readonly CultureInfo Colombia = new("es-CO");

    public static string Pesos(this decimal valor) => "$" + valor.ToString("N0", Colombia);

    public static string Fecha(this DateTime fecha) => fecha.ToString("dd/MM/yyyy");
}
