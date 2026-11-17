namespace Core.Entidades;

public class Huesped : Persona
{
    /// <summary>Id del usuario de Identity asociado (si el huésped tiene cuenta web).</summary>
    public string? UsuarioId { get; set; }

    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
