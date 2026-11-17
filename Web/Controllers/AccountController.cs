using Core.Constantes;
using Core.Entidades;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Web.Models;

namespace Web.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IRepositorio<Huesped> _huespedes;

    public AccountController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IRepositorio<Huesped> huespedes)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _huespedes = huespedes;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null) => View(new LoginVM { ReturnUrl = returnUrl });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVM modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        var resultado = await _signInManager.PasswordSignInAsync(
            modelo.Email, modelo.Password, modelo.Recordarme, lockoutOnFailure: false);

        if (resultado.Succeeded)
        {
            if (!string.IsNullOrEmpty(modelo.ReturnUrl) && Url.IsLocalUrl(modelo.ReturnUrl))
                return Redirect(modelo.ReturnUrl);

            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
        return View(modelo);
    }

    [HttpGet]
    public IActionResult Registro() => View(new RegistroVM());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Registro(RegistroVM modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        string documento = modelo.Documento.Trim();

        // Puede que recepción ya haya registrado al huésped desde el escritorio.
        var existente = (await _huespedes.BuscarAsync(h => h.Documento == documento)).FirstOrDefault();
        if (existente?.UsuarioId is not null)
        {
            ModelState.AddModelError(nameof(modelo.Documento), "Ya existe una cuenta con ese documento.");
            return View(modelo);
        }

        var usuario = new IdentityUser { UserName = modelo.Email, Email = modelo.Email };
        var resultado = await _userManager.CreateAsync(usuario, modelo.Password);

        if (!resultado.Succeeded)
        {
            foreach (var error in resultado.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(modelo);
        }

        await _userManager.AddToRoleAsync(usuario, Roles.Huesped);

        if (existente is null)
        {
            await _huespedes.AgregarAsync(new Huesped
            {
                Nombre = modelo.Nombre.Trim(),
                Documento = documento,
                Email = modelo.Email,
                Telefono = modelo.Telefono,
                UsuarioId = usuario.Id
            });
        }
        else
        {
            existente.UsuarioId = usuario.Id;
            await _huespedes.ActualizarAsync(existente);
        }

        await _signInManager.SignInAsync(usuario, isPersistent: false);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccesoDenegado() => View();
}
