using AutoMapper;
using Veterinaria.Datos.Repositories;
using Veterinaria.Entidades.DTOs;
using Veterinaria.Entidades.Models;
using Veterinaria.Negocio.Exceptions;
namespace Veterinaria.Negocio.Services;
public sealed class MascotaService(IMascotaRepository repo, IMapper mapper) : IMascotaService
{
    public async Task<IReadOnlyList<MascotaDto>> ObtenerAsync(string? especie, int? duenoId, CancellationToken ct = default) => mapper.Map<List<MascotaDto>>(await repo.ObtenerAsync(especie, duenoId, ct));
    public async Task<MascotaDto?> ObtenerPorIdAsync(int id, CancellationToken ct = default) => mapper.Map<MascotaDto?>(await repo.ObtenerPorIdAsync(id, ct));
    public async Task<IReadOnlyList<MascotaDto>?> ObtenerPorDuenoAsync(int duenoId, CancellationToken ct = default)
    {
        var result = await repo.ObtenerAsync(null, duenoId, ct);
        return result.Count == 0 && !await repo.ExisteDuenoAsync(duenoId, ct) ? null : mapper.Map<List<MascotaDto>>(result);
    }
    public async Task<MascotaDto> CrearAsync(MascotaCrearDto dto, CancellationToken ct = default)
    {
        if (dto.FechaNacimiento > DateOnly.FromDateTime(DateTime.Today)) throw new ReglaNegocioException("La fecha de nacimiento no puede estar en el futuro.");
        if (!await repo.ExisteDuenoAsync(dto.DuenoId, ct)) throw new ReglaNegocioException("El dueño indicado no existe.");
        var entity = mapper.Map<Mascota>(dto); await repo.AgregarAsync(entity, ct); await repo.GuardarCambiosAsync(ct);
        var saved = await repo.ObtenerPorIdAsync(entity.MascotaId, ct); return mapper.Map<MascotaDto>(saved);
    }
    public async Task<bool> ActualizarAsync(int id, MascotaActualizarDto dto, CancellationToken ct = default)
    {
        var entity = await repo.ObtenerPorIdAsync(id, ct); if (entity is null) return false;
        if (dto.FechaNacimiento > DateOnly.FromDateTime(DateTime.Today)) throw new ReglaNegocioException("La fecha de nacimiento no puede estar en el futuro.");
        if (!await repo.ExisteDuenoAsync(dto.DuenoId, ct)) throw new ReglaNegocioException("El dueño indicado no existe.");
        mapper.Map(dto, entity); await repo.GuardarCambiosAsync(ct); return true;
    }
    public async Task<bool> EliminarAsync(int id, CancellationToken ct = default)
    {
        var entity = await repo.ObtenerPorIdAsync(id, ct); if (entity is null) return false;
        repo.Eliminar(entity); await repo.GuardarCambiosAsync(ct); return true;
    }
}
