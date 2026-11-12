using AutoMapper;
using Veterinaria.Datos.Repositories;
using Veterinaria.Entidades.DTOs;
using Veterinaria.Entidades.Models;
using Veterinaria.Negocio.Exceptions;
namespace Veterinaria.Negocio.Services;
public sealed class DuenoService(IDuenoRepository repo, IMapper mapper) : IDuenoService
{
    public async Task<IReadOnlyList<DuenoDto>> ObtenerTodosAsync(CancellationToken ct = default) => mapper.Map<List<DuenoDto>>(await repo.ObtenerTodosAsync(ct));
    public async Task<DuenoDto?> ObtenerPorIdAsync(int id, CancellationToken ct = default) => mapper.Map<DuenoDto?>(await repo.ObtenerPorIdAsync(id, ct));
    public async Task<DuenoDto> CrearAsync(DuenoCrearDto dto, CancellationToken ct = default)
    {
        if (await repo.ObtenerPorCorreoAsync(dto.Correo, ct) is not null) throw new ReglaNegocioException("Ya existe un dueño registrado con ese correo.");
        var entity = mapper.Map<Dueno>(dto); await repo.AgregarAsync(entity, ct); await repo.GuardarCambiosAsync(ct); return mapper.Map<DuenoDto>(entity);
    }
    public async Task<bool> ActualizarAsync(int id, DuenoActualizarDto dto, CancellationToken ct = default)
    {
        var entity = await repo.ObtenerPorIdAsync(id, ct); if (entity is null) return false;
        var duplicado = await repo.ObtenerPorCorreoAsync(dto.Correo, ct); if (duplicado is not null && duplicado.DuenoId != id) throw new ReglaNegocioException("El correo ya pertenece a otro dueño.");
        mapper.Map(dto, entity); await repo.GuardarCambiosAsync(ct); return true;
    }
    public async Task<bool> EliminarAsync(int id, CancellationToken ct = default)
    {
        var entity = await repo.ObtenerPorIdAsync(id, ct); if (entity is null) return false;
        if (entity.Mascotas.Count > 0) throw new ReglaNegocioException("No se puede eliminar un dueño que todavía tiene mascotas registradas.");
        repo.Eliminar(entity); await repo.GuardarCambiosAsync(ct); return true;
    }
}
