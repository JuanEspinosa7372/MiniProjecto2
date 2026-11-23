using Core.Constantes;
using Core.Entidades;
using Core.Excepciones;
using Core.Interfaces;
using Core.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Web.Helpers;
using Web.Models;

namespace Web.Controllers;

[Authorize]
public class ReservasController : Controller
{
    private readonly ReservaServicio _servicio;
    private readonly IRepositorio<Reserva> _reservas;
    private readonly IRepositorio<Huesped> _huespedes;
    private readonly UserManager<IdentityUser> _userManager;

    public ReservasController(
        ReservaServicio servicio,
        IRepositorio<Reserva> reservas,
        IRepositorio<Huesped> huespedes,
        UserManager<IdentityUser> userManager)
    {
        _servicio = servicio;
        _reservas = reservas;
        _huespedes = huespedes;
        _userManager = userManager;
    }

    private bool EsPersonal => User.IsInRole(Roles.Admin) || User.IsInRole(Roles.Recepcionista);

    // El personal ve todas las reservas; el huésped solo las suyas.
    public async Task<IActionResult> Index()
    {
        IReadOnlyList<Reserva> reservas;

        if (EsPersonal)
        {
            reservas = await _reservas.ObtenerTodosAsync(r => r.Huesped!, r => r.Habitacion!);
        }
        else
        {
            string? usuarioId = _userManager.GetUserId(User);
            reservas = await _reservas.BuscarAsync(
                r => r.Huesped!.UsuarioId == usuarioId, r => r.Huesped!, r => r.Habitacion!);
        }

        return View(reservas.OrderByDescending(r => r.FechaEntrada).ToList());
    }

    // ---------- Nueva reserva (solo huéspedes)

    [Authorize(Roles = Roles.Huesped)]
    [HttpGet]
    public IActionResult Nueva() => View(new DisponibilidadVM());

    [Authorize(Roles = Roles.Huesped)]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Nueva(DisponibilidadVM modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        try
        {
            var disponibles = await _servicio.BuscarDisponiblesAsync(modelo.Entrada, modelo.Salida, modelo.Personas);

            modelo.Habitaciones = disponibles;
            modelo.Totales = disponibles.ToDictionary(
                h => h.Id,
                h => _servicio.CalcularTotal(h.PrecioNoche, modelo.Entrada, modelo.Salida));
        }
        catch (ReglaNegocioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        return View(modelo);
    }

    [Authorize(Roles = Roles.Huesped)]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(int habitacionId, DateTime entrada, DateTime salida, int personas)
    {
        string? usuarioId = _userManager.GetUserId(User);
        var huesped = (await _huespedes.BuscarAsync(h => h.UsuarioId == usuarioId)).FirstOrDefault();

        if (huesped is null)
        {
            TempData["Error"] = "No se encontró tu perfil de huésped.";
            return RedirectToAction(nameof(Nueva));
        }

        try
        {
            var reserva = await _servicio.CrearReservaAsync(huesped.Id, habitacionId, entrada, salida, personas);
            TempData["Exito"] = $"Reserva #{reserva.Id} confirmada. Total: {reserva.Total.Pesos()}";
            return RedirectToAction(nameof(Index));
        }
        catch (ReglaNegocioException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Nueva));
        }
    }

    // ---------- Acciones sobre una reserva

    [Authorize(Roles = Roles.Personal)]
    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> CheckIn(int id) =>
        EjecutarAsync(() => _servicio.CheckInAsync(id), "Check-in realizado.");

    [Authorize(Roles = Roles.Personal)]
    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> CheckOut(int id) =>
        EjecutarAsync(() => _servicio.CheckOutAsync(id), "Check-out realizado.");

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        if (!EsPersonal)
        {
            // Un huésped solo puede cancelar sus propias reservas.
            var reserva = await _reservas.ObtenerPorIdAsync(id, r => r.Huesped!);
            if (reserva?.Huesped?.UsuarioId != _userManager.GetUserId(User))
                return Forbid();
        }

        return await EjecutarAsync(() => _servicio.CancelarAsync(id), "Reserva cancelada.");
    }

    private async Task<IActionResult> EjecutarAsync(Func<Task> accion, string mensajeOk)
    {
        try
        {
            await accion();
            TempData["Exito"] = mensajeOk;
        }
        catch (ReglaNegocioException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
