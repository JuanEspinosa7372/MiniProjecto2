using Core.Entidades;
using Core.Enums;
using Core.Excepciones;
using Core.Interfaces;

namespace Core.Servicios;

public class ReservaEventArgs : EventArgs
{
    public Reserva Reserva { get; }
    public ReservaEventArgs(Reserva reserva) => Reserva = reserva;
}

/// <summary>Lógica de negocio de las reservas: disponibilidad, tarifas, check-in/out.</summary>
public class ReservaServicio
{
    private readonly IRepositorio<Reserva> _reservas;
    private readonly IRepositorio<Habitacion> _habitaciones;
    private readonly IRepositorio<Huesped> _huespedes;
    private readonly IEnumerable<ITarifa> _tarifas;

    // Eventos: la interfaz (web o escritorio) puede suscribirse para notificar.
    public event EventHandler<ReservaEventArgs>? ReservaCreada;
    public event EventHandler<ReservaEventArgs>? CheckInRealizado;
    public event EventHandler<ReservaEventArgs>? CheckOutRealizado;

    public ReservaServicio(
        IRepositorio<Reserva> reservas,
        IRepositorio<Habitacion> habitaciones,
        IRepositorio<Huesped> huespedes,
        IEnumerable<ITarifa> tarifas)
    {
        _reservas = reservas;
        _habitaciones = habitaciones;
        _huespedes = huespedes;
        _tarifas = tarifas;
    }

    public async Task<IReadOnlyList<Habitacion>> BuscarDisponiblesAsync(DateTime entrada, DateTime salida, int personas)
    {
        entrada = entrada.Date;
        salida = salida.Date;
        ValidarFechas(entrada, salida);

        var candidatas = await _habitaciones.BuscarAsync(
            h => h.Estado == EstadoHabitacion.Disponible && h.Capacidad >= personas);

        // Reservas que se cruzan con el rango: A.entrada < B.salida && B.entrada < A.salida
        var enConflicto = await _reservas.BuscarAsync(
            r => r.Estado != EstadoReserva.Cancelada
              && r.Estado != EstadoReserva.CheckOut
              && r.FechaEntrada < salida
              && entrada < r.FechaSalida);

        // HashSet: búsqueda O(1) para saber si una habitación está ocupada.
        var ocupadas = enConflicto.Select(r => r.HabitacionId).ToHashSet();

        return candidatas
            .Where(h => !ocupadas.Contains(h.Id))
            .OrderBy(h => h.PrecioNoche)
            .ToList();
    }

    public decimal CalcularTotal(decimal precioNoche, DateTime entrada, DateTime salida)
    {
        int noches = (salida.Date - entrada.Date).Days;
        decimal subtotal = precioNoche * noches;

        // Aplica cada regla de tarifa en cadena (LINQ Aggregate).
        decimal total = _tarifas.Aggregate(subtotal, (acum, tarifa) => tarifa.Aplicar(acum, entrada, noches));
        return Math.Round(total, 0);
    }

    public async Task<Reserva> CrearReservaAsync(
        int huespedId, int habitacionId, DateTime entrada, DateTime salida, int personas)
    {
        entrada = entrada.Date;
        salida = salida.Date;
        ValidarFechas(entrada, salida);

        var huesped = await _huespedes.ObtenerPorIdAsync(huespedId)
            ?? throw new ReglaNegocioException("El huésped no existe.");
        var habitacion = await _habitaciones.ObtenerPorIdAsync(habitacionId)
            ?? throw new ReglaNegocioException("La habitación no existe.");

        if (habitacion.Estado != EstadoHabitacion.Disponible)
            throw new ReglaNegocioException($"La habitación {habitacion.Numero} está en mantenimiento.");

        if (personas < 1 || personas > habitacion.Capacidad)
            throw new ReglaNegocioException($"La habitación {habitacion.Numero} admite máximo {habitacion.Capacidad} persona(s).");

        var cruces = await _reservas.BuscarAsync(
            r => r.HabitacionId == habitacionId
              && r.Estado != EstadoReserva.Cancelada
              && r.Estado != EstadoReserva.CheckOut
              && r.FechaEntrada < salida
              && entrada < r.FechaSalida);

        if (cruces.Any())
            throw new ReglaNegocioException($"La habitación {habitacion.Numero} ya está reservada en esas fechas.");

        var reserva = new Reserva
        {
            HuespedId = huesped.Id,
            HabitacionId = habitacion.Id,
            FechaEntrada = entrada,
            FechaSalida = salida,
            NumeroHuespedes = personas,
            Estado = EstadoReserva.Confirmada,
            Total = CalcularTotal(habitacion.PrecioNoche, entrada, salida)
        };

        await _reservas.AgregarAsync(reserva);
        ReservaCreada?.Invoke(this, new ReservaEventArgs(reserva));
        return reserva;
    }

    public async Task CheckInAsync(int reservaId)
    {
        var reserva = await ObtenerReservaAsync(reservaId);

        if (reserva.Estado != EstadoReserva.Confirmada)
            throw new ReglaNegocioException("Solo se puede hacer check-in a una reserva confirmada.");
        if (DateTime.Today < reserva.FechaEntrada.Date)
            throw new ReglaNegocioException("Aún no es la fecha de entrada de la reserva.");

        reserva.Estado = EstadoReserva.CheckIn;
        await _reservas.ActualizarAsync(reserva);
        CheckInRealizado?.Invoke(this, new ReservaEventArgs(reserva));
    }

    public async Task CheckOutAsync(int reservaId)
    {
        var reserva = await ObtenerReservaAsync(reservaId);

        if (reserva.Estado != EstadoReserva.CheckIn)
            throw new ReglaNegocioException("Solo se puede hacer check-out a una reserva con check-in.");

        reserva.Estado = EstadoReserva.CheckOut;
        await _reservas.ActualizarAsync(reserva);
        CheckOutRealizado?.Invoke(this, new ReservaEventArgs(reserva));
    }

    public async Task CancelarAsync(int reservaId)
    {
        var reserva = await ObtenerReservaAsync(reservaId);

        if (reserva.Estado is not (EstadoReserva.Pendiente or EstadoReserva.Confirmada))
            throw new ReglaNegocioException("Solo se pueden cancelar reservas pendientes o confirmadas.");

        reserva.Estado = EstadoReserva.Cancelada;
        await _reservas.ActualizarAsync(reserva);
    }

    private async Task<Reserva> ObtenerReservaAsync(int id) =>
        await _reservas.ObtenerPorIdAsync(id)
            ?? throw new ReglaNegocioException("La reserva no existe.");

    private static void ValidarFechas(DateTime entrada, DateTime salida)
    {
        if (entrada < DateTime.Today)
            throw new ReglaNegocioException("La fecha de entrada no puede ser anterior a hoy.");
        if (salida <= entrada)
            throw new ReglaNegocioException("La fecha de salida debe ser posterior a la de entrada.");
    }
}
