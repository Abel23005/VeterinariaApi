namespace Veterinaria.Entidades.Models;

public partial class Dueno
{
    public int DuenoId { get; set; }
    public string Nombre { get; set; } = null!;
    public string Correo { get; set; } = null!;
    public string Telefono { get; set; } = null!;
    public virtual ICollection<Mascota> Mascotas { get; set; } = new List<Mascota>();
}
