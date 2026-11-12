using Veterinaria.Entidades.DTOs;
namespace Veterinaria.Negocio.Services;
public interface IDuenoService
{
    Task<IReadOnlyList<DuenoDto>> ObtenerTodosAsync(CancellationToken ct = default);
    Task<DuenoDto?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<DuenoDto> CrearAsync(DuenoCrearDto dto, CancellationToken ct = default);
    Task<bool> ActualizarAsync(int id, DuenoActualizarDto dto, CancellationToken ct = default);
    Task<bool> EliminarAsync(int id, CancellationToken ct = default);
}
