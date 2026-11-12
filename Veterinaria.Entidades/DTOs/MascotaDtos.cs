using System.ComponentModel.DataAnnotations;

namespace Veterinaria.Entidades.DTOs;

public sealed record MascotaDto(int MascotaId, string Nombre, string Especie, string Raza, DateOnly FechaNacimiento, int DuenoId, string NombreDueno);
public sealed class MascotaCrearDto
{
    [Required, StringLength(80, MinimumLength = 2)] public string Nombre { get; set; } = string.Empty;
    [Required, StringLength(40)] public string Especie { get; set; } = string.Empty;
    [Required, StringLength(60)] public string Raza { get; set; } = string.Empty;
    [Required, DataType(DataType.Date)] public DateOnly FechaNacimiento { get; set; }
    [Range(1, int.MaxValue)] public int DuenoId { get; set; }
}
public sealed class MascotaActualizarDto
{
    [Required, StringLength(80, MinimumLength = 2)] public string Nombre { get; set; } = string.Empty;
    [Required, StringLength(40)] public string Especie { get; set; } = string.Empty;
    [Required, StringLength(60)] public string Raza { get; set; } = string.Empty;
    [Required, DataType(DataType.Date)] public DateOnly FechaNacimiento { get; set; }
    [Range(1, int.MaxValue)] public int DuenoId { get; set; }
}
