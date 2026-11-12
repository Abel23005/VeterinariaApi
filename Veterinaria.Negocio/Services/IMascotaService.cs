using Veterinaria.Entidades.DTOs;
namespace Veterinaria.Negocio.Services;
public interface IMascotaService
{
    Task<IReadOnlyList<MascotaDto>> ObtenerAsync(string? especie, int? duenoId, CancellationToken ct = default);
    Task<MascotaDto?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<MascotaDto>?> ObtenerPorDuenoAsync(int duenoId, CancellationToken ct = default);
    Task<MascotaDto> CrearAsync(MascotaCrearDto dto, CancellationToken ct = default);
    Task<bool> ActualizarAsync(int id, MascotaActualizarDto dto, CancellationToken ct = default);
    Task<bool> EliminarAsync(int id, CancellationToken ct = default);
}
