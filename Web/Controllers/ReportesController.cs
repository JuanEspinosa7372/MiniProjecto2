using Core.Constantes;
using Core.Entidades;
using Core.Enums;
using Core.Interfaces;
using Core.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Models;

namespace Web.Controllers;

[Authorize(Roles = Roles.Admin)]
public class ReportesController : Controller
{
    private readonly IRepositorio<Reserva> _reservas;

    public ReportesController(IRepositorio<Reserva> reservas) => _reservas = reservas;

    public async Task<IActionResult> Index()
    {
        var reservas = await _reservas.ObtenerTodosAsync(r => r.Habitacion!);
        var vigentes = reservas.Where(r => r.Estado != EstadoReserva.Cancelada).ToList();

        // LINQ: GroupBy + ToDictionary. Los decimales se suman en memoria (SQLite no suma decimales en SQL).
        var ingresosPorMes = vigentes
            .GroupBy(r => r.FechaEntrada.ToString("yyyy-MM"))
            .ToDictionary(g => g.Key, g => g.Sum(r => r.Total));

        var modelo = new ReporteVM
        {
            IngresoTotal = vigentes.Sum(r => r.Total),
            IngresosPorMes = new SortedDictionary<string, decimal>(ingresosPorMes),
            ReservasPorEstado = reservas
                .GroupBy(r => r.Estado.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),
            ReservasPorTipo = vigentes
                .GroupBy(r => r.Habitacion?.Tipo.ToString() ?? "Sin dato")
                .ToDictionary(g => g.Key, g => g.Count()),
            Comparaciones = ComparadorEstructuras.Ejecutar()
        };

        return View(modelo);
    }
}
