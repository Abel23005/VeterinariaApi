using System.ComponentModel.DataAnnotations;

namespace Veterinaria.Entidades.DTOs;

public sealed record DuenoDto(int DuenoId, string Nombre, string Correo, string Telefono, int CantidadMascotas);
public sealed class DuenoCrearDto
{
    [Required, StringLength(100, MinimumLength = 2)] public string Nombre { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(150)] public string Correo { get; set; } = string.Empty;
    [Required, StringLength(20, MinimumLength = 7)] public string Telefono { get; set; } = string.Empty;
}
public sealed class DuenoActualizarDto
{
    [Required, StringLength(100, MinimumLength = 2)] public string Nombre { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(150)] public string Correo { get; set; } = string.Empty;
    [Required, StringLength(20, MinimumLength = 7)] public string Telefono { get; set; } = string.Empty;
}
