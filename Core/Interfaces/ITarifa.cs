namespace Core.Interfaces;

/// <summary>Regla de tarifa (polimorfismo): cada implementación ajusta el subtotal.</summary>
public interface ITarifa
{
    string Nombre { get; }
    decimal Aplicar(decimal subtotal, DateTime entrada, int noches);
}
