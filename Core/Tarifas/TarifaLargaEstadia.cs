using Core.Interfaces;

namespace Core.Tarifas;

/// <summary>Descuento del 10% para estadías de 7 noches o más.</summary>
public class TarifaLargaEstadia : ITarifa
{
    public string Nombre => "Descuento larga estadía (10%)";

    public decimal Aplicar(decimal subtotal, DateTime entrada, int noches) =>
        noches >= 7 ? subtotal * 0.90m : subtotal;
}
