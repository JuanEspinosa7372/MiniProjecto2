namespace Core.Excepciones;

/// <summary>Se lanza cuando se viola una regla de negocio (cruce de fechas, capacidad, etc.).</summary>
public class ReglaNegocioException : Exception
{
    public ReglaNegocioException(string mensaje) : base(mensaje) { }
}
