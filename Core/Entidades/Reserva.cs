using Core.Enums;

namespace Core.Entidades;

public class Reserva : EntidadBase
{
    public int HuespedId { get; set; }
    public Huesped? Huesped { get; set; }

    public int HabitacionId { get; set; }
    public Habitacion? Habitacion { get; set; }

    public DateTime FechaEntrada { get; set; }
    public DateTime FechaSalida { get; set; }
    public int NumeroHuespedes { get; set; } = 1;
    public EstadoReserva Estado { get; set; } = EstadoReserva.Pendiente;
    public decimal Total { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // Propiedades calculadas (sin setter): EF Core no las mapea.
    public int Noches => (FechaSalida.Date - FechaEntrada.Date).Days;

    public bool OcupaHabitacion =>
        Estado is EstadoReserva.Pendiente or EstadoReserva.Confirmada or EstadoReserva.CheckIn;
}
