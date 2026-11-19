using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Veterinaria.Entidades.DTOs;
using Veterinaria.Negocio.Exceptions;
using Veterinaria.Negocio.Services;
namespace Veterinaria.Api.Controllers;
[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class DuenosController(IDuenoService service, IMascotaService mascotas) : ControllerBase
{
    [HttpGet] public async Task<ActionResult<IReadOnlyList<DuenoDto>>> Get(CancellationToken ct) => Ok(await service.ObtenerTodosAsync(ct));
    [HttpGet("{id:int}")] public async Task<ActionResult<DuenoDto>> GetById(int id, CancellationToken ct) { var item = await service.ObtenerPorIdAsync(id, ct); return item is null ? NotFound() : Ok(item); }
    [HttpPost, Authorize(Roles = "Recepcionista,Veterinario")] public async Task<ActionResult<DuenoDto>> Post(DuenoCrearDto dto, CancellationToken ct)
    { try { var item = await service.CrearAsync(dto, ct); return CreatedAtAction(nameof(GetById), new { id = item.DuenoId }, item); } catch (ReglaNegocioException ex) { return Conflict(new { error = ex.Message }); } }
    [HttpPut("{id:int}"), Authorize(Roles = "Recepcionista,Veterinario")] public async Task<IActionResult> Put(int id, DuenoActualizarDto dto, CancellationToken ct)
    { try { return await service.ActualizarAsync(id, dto, ct) ? NoContent() : NotFound(); } catch (ReglaNegocioException ex) { return Conflict(new { error = ex.Message }); } }
    [HttpDelete("{id:int}"), Authorize(Roles = "Veterinario")] public async Task<IActionResult> Delete(int id, CancellationToken ct)
    { try { return await service.EliminarAsync(id, ct) ? NoContent() : NotFound(); } catch (ReglaNegocioException ex) { return Conflict(new { error = ex.Message }); } }
    [HttpGet("{id:int}/mascotas")] public async Task<ActionResult<IReadOnlyList<MascotaDto>>> MascotasDelDueno(int id, CancellationToken ct)
    { var result = await mascotas.ObtenerPorDuenoAsync(id, ct); return result is null ? NotFound() : Ok(result); }
}
