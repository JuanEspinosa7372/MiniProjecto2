using Core.Constantes;
using Core.Entidades;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[Authorize(Roles = Roles.Personal)]
public class HabitacionesController : Controller
{
    private readonly IRepositorio<Habitacion> _habitaciones;

    public HabitacionesController(IRepositorio<Habitacion> habitaciones) => _habitaciones = habitaciones;

    public async Task<IActionResult> Index()
    {
        var lista = await _habitaciones.ObtenerTodosAsync();
        return View(lista.OrderBy(h => h.Numero).ToList());
    }

    [Authorize(Roles = Roles.Admin)]
    public IActionResult Create() => View(new Habitacion());

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Create([Bind("Numero,Tipo,Capacidad,PrecioNoche,Estado")] Habitacion habitacion)
    {
        if (await NumeroDuplicadoAsync(habitacion))
            ModelState.AddModelError(nameof(habitacion.Numero), "Ya existe una habitación con ese número.");

        if (!ModelState.IsValid) return View(habitacion);

        await _habitaciones.AgregarAsync(habitacion);
        TempData["Exito"] = $"Habitación {habitacion.Numero} creada.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Edit(int id)
    {
        var habitacion = await _habitaciones.ObtenerPorIdAsync(id);
        if (habitacion is null) return NotFound();
        return View(habitacion);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Edit([Bind("Id,Numero,Tipo,Capacidad,PrecioNoche,Estado")] Habitacion modelo)
    {
        if (await NumeroDuplicadoAsync(modelo))
            ModelState.AddModelError(nameof(modelo.Numero), "Ya existe una habitación con ese número.");

        if (!ModelState.IsValid) return View(modelo);

        var habitacion = await _habitaciones.ObtenerPorIdAsync(modelo.Id);
        if (habitacion is null) return NotFound();

        habitacion.Numero = modelo.Numero;
        habitacion.Tipo = modelo.Tipo;
        habitacion.Capacidad = modelo.Capacidad;
        habitacion.PrecioNoche = modelo.PrecioNoche;
        habitacion.Estado = modelo.Estado;

        await _habitaciones.ActualizarAsync(habitacion);
        TempData["Exito"] = $"Habitación {habitacion.Numero} actualizada.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> NumeroDuplicadoAsync(Habitacion habitacion)
    {
        string numero = habitacion.Numero;
        int id = habitacion.Id;
        var existentes = await _habitaciones.BuscarAsync(h => h.Numero == numero && h.Id != id);
        return existentes.Any();
    }
}
