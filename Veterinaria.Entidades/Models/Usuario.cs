namespace Veterinaria.Entidades.Models;
public partial class Usuario { public int UsuarioId { get; set; } public string Correo { get; set; } = null!; public string ClaveHash { get; set; } = null!; public string Rol { get; set; } = null!; }
