using System.ComponentModel.DataAnnotations;
using Core.Enums;

namespace Core.Entidades;

public class Habitacion : EntidadBase
{
    [Required, StringLength(10)]
    public string Numero { get; set; } = string.Empty;

    public TipoHabitacion Tipo { get; set; }

    [Range(1, 10)]
    public int Capacidad { get; set; } = 1;

    [Range(typeof(decimal), "1", "100000000")]
    public decimal PrecioNoche { get; set; }

    public EstadoHabitacion Estado { get; set; } = EstadoHabitacion.Disponible;

    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

    public override string ToString() => $"{Numero} ({Tipo})";
}
