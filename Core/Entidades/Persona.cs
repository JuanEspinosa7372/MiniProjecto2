using System.ComponentModel.DataAnnotations;

namespace Core.Entidades;

/// <summary>Clase abstracta con los datos comunes de una persona (herencia).</summary>
public abstract class Persona : EntidadBase
{
    [Required, StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string Documento { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    public string? Telefono { get; set; }
}
