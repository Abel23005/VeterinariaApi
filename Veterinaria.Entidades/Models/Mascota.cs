namespace Veterinaria.Entidades.Models;

public partial class Mascota
{
    public int MascotaId { get; set; }
    public string Nombre { get; set; } = null!;
    public string Especie { get; set; } = null!;
    public string Raza { get; set; } = null!;
    public DateOnly FechaNacimiento { get; set; }
    public int DuenoId { get; set; }
    public virtual Dueno Dueno { get; set; } = null!;
}
