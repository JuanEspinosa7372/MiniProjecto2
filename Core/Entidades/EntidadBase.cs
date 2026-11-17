namespace Core.Entidades;

/// <summary>Clase base abstracta: todas las entidades persistentes tienen un Id.</summary>
public abstract class EntidadBase
{
    public int Id { get; set; }
}
