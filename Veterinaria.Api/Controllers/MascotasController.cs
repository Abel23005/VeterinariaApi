using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Veterinaria.Entidades.DTOs;
using Veterinaria.Negocio.Exceptions;
using Veterinaria.Negocio.Services;
namespace Veterinaria.Api.Controllers;
[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class MascotasController(IMascotaService service) : ControllerBase
{
    [HttpGet] public async Task<ActionResult<IReadOnlyList<MascotaDto>>> Get([FromQuery] string? especie, [FromQuery] int? duenoId, CancellationToken ct) => Ok(await service.ObtenerAsync(especie, duenoId, ct));
    [HttpGet("{id:int}")] public async Task<ActionResult<MascotaDto>> GetById(int id, CancellationToken ct) { var item = await service.ObtenerPorIdAsync(id, ct); return item is null ? NotFound() : Ok(item); }
    [HttpPost, Authorize(Roles = "Recepcionista,Veterinario")] public async Task<ActionResult<MascotaDto>> Post(MascotaCrearDto dto, CancellationToken ct)
    { try { var item = await service.CrearAsync(dto, ct); return CreatedAtAction(nameof(GetById), new { id = item.MascotaId }, item); } catch (ReglaNegocioException ex) { return Conflict(new { error = ex.Message }); } }
    [HttpPut("{id:int}"), Authorize(Roles = "Veterinario")] public async Task<IActionResult> Put(int id, MascotaActualizarDto dto, CancellationToken ct)
    { try { return await service.ActualizarAsync(id, dto, ct) ? NoContent() : NotFound(); } catch (ReglaNegocioException ex) { return Conflict(new { error = ex.Message }); } }
    [HttpDelete("{id:int}"), Authorize(Roles = "Veterinario")] public async Task<IActionResult> Delete(int id, CancellationToken ct) => await service.EliminarAsync(id, ct) ? NoContent() : NotFound();
}
