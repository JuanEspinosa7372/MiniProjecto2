using Core.Interfaces;

namespace Core.Tarifas;

/// <summary>Recargo del 20% si la entrada es en enero, junio, julio o diciembre.</summary>
public class TarifaTemporadaAlta : ITarifa
{
    private static readonly int[] MesesAltos = { 1, 6, 7, 12 };

    public string Nombre => "Recargo temporada alta (20%)";

    public decimal Aplicar(decimal subtotal, DateTime entrada, int noches) =>
        MesesAltos.Contains(entrada.Month) ? subtotal * 1.20m : subtotal;
}
