using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Propietarios.Api.Models;

namespace Propietarios.Api.Controllers;

[ApiController]
[Route("api/propietarios")]
[Authorize]
public sealed class PropietariosController(IPropietarioStore store) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyList<Propietario>> Get() => Ok(store.ObtenerTodos());

    [HttpGet("{id:int}")]
    public ActionResult<Propietario> GetById(int id)
    {
        var propietario = store.ObtenerPorId(id);
        return propietario is null ? NotFound() : Ok(propietario);
    }
}
