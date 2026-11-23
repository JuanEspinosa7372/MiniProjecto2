using System.Diagnostics;
using Core.Entidades;

namespace Core.Servicios;

public record ResultadoComparacion(string Prueba, string Estructura, string Complejidad, double Milisegundos);

/// <summary>
/// Mide el tiempo de distintas estructuras de datos para justificar la decisión final
/// (se muestra en el reporte del administrador).
/// </summary>
public static class ComparadorEstructuras
{
    public static IReadOnlyList<ResultadoComparacion> Ejecutar(int elementos = 20000, int busquedas = 5000)
    {
        var aleatorio = new Random(42); // semilla fija: resultados comparables
        var ids = Enumerable.Range(0, busquedas).Select(_ => aleatorio.Next(1, elementos + 1)).ToArray();

        // Prueba 1: buscar una habitación por su Id
        var lista = Enumerable.Range(1, elementos).Select(i => new Habitacion { Id = i, Numero = i.ToString() }).ToList();
        var diccionario = lista.ToDictionary(h => h.Id);

        int hallados = 0;
        double tList = Medir(() => { foreach (var id in ids) if (lista.Find(h => h.Id == id) is not null) hallados++; });
        double tDic = Medir(() => { foreach (var id in ids) if (diccionario.TryGetValue(id, out _)) hallados++; });

        // Prueba 2: saber si una habitación está ocupada
        var ocupadasLista = Enumerable.Range(1, elementos).Where(i => i % 3 == 0).ToList();
        var ocupadasHash = ocupadasLista.ToHashSet();

        double tContains = Medir(() => { foreach (var id in ids) if (ocupadasLista.Contains(id)) hallados++; });
        double tHash = Medir(() => { foreach (var id in ids) if (ocupadasHash.Contains(id)) hallados++; });

        GC.KeepAlive(hallados);

        return new List<ResultadoComparacion>
        {
            new("Buscar habitación por Id", "List<T>.Find", "O(n)", tList),
            new("Buscar habitación por Id", "Dictionary<int, T>", "O(1)", tDic),
            new("¿Habitación ocupada?", "List<int>.Contains", "O(n)", tContains),
            new("¿Habitación ocupada?", "HashSet<int>", "O(1)", tHash)
        };
    }

    private static double Medir(Action accion)
    {
        var reloj = Stopwatch.StartNew();
        accion();
        reloj.Stop();
        return reloj.Elapsed.TotalMilliseconds;
    }
}
