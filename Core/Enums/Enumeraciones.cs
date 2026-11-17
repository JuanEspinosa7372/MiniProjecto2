namespace Core.Enums;

public enum TipoHabitacion
{
    Sencilla = 1,
    Doble = 2,
    Suite = 3
}

// "Ocupada" no se guarda: se deduce de las reservas con check-in.
public enum EstadoHabitacion
{
    Disponible = 1,
    Mantenimiento = 2
}

public enum EstadoReserva
{
    Pendiente = 1,
    Confirmada = 2,
    CheckIn = 3,
    CheckOut = 4,
    Cancelada = 5
}
