using System.ComponentModel.DataAnnotations;
using Core.Entidades;

namespace Web.Models;

public class DisponibilidadVM
{
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Display(Name = "Entrada")]
    public DateTime Entrada { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Display(Name = "Salida")]
    public DateTime Salida { get; set; } = DateTime.Today.AddDays(1);

    [Range(1, 10, ErrorMessage = "Entre 1 y 10 personas.")]
    [Display(Name = "Personas")]
    public int Personas { get; set; } = 1;

    public IReadOnlyList<Habitacion>? Habitaciones { get; set; }

    // Id de habitación -> total estimado de la estadía
    public Dictionary<int, decimal> Totales { get; set; } = new();
}
