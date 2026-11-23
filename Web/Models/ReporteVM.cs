using Core.Servicios;

namespace Web.Models;

public class ReporteVM
{
    public decimal IngresoTotal { get; set; }

    // SortedDictionary: las llaves "yyyy-MM" quedan ordenadas cronológicamente
    public SortedDictionary<string, decimal> IngresosPorMes { get; set; } = new();

    public Dictionary<string, int> ReservasPorEstado { get; set; } = new();
    public Dictionary<string, int> ReservasPorTipo { get; set; } = new();

    public IReadOnlyList<ResultadoComparacion> Comparaciones { get; set; } = Array.Empty<ResultadoComparacion>();
}
