using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Veterinaria.Entidades.DTOs;
using Veterinaria.Negocio.Exceptions;
using Veterinaria.Negocio.Services;
namespace Veterinaria.Api.Controllers;
[ApiController,Route("api/auth")]
public sealed class AuthController(IAuthService auth):ControllerBase {
 [AllowAnonymous,HttpPost("registro")] public async Task<IActionResult> Registro(RegistroDto dto,CancellationToken ct){try{return StatusCode(201,await auth.RegistrarAsync(dto,ct));}catch(ReglaNegocioException ex){return Conflict(new{error=ex.Message});}}
 [AllowAnonymous,HttpPost("login")] public async Task<IActionResult> Login(LoginDto dto,CancellationToken ct){var token=await auth.LoginAsync(dto,ct);return token is null?Unauthorized():Ok(token);}
 [Authorize,HttpGet("perfil")] public IActionResult Perfil()=>Ok(new PerfilDto(User.FindFirstValue("email")??User.FindFirstValue(ClaimTypes.Email)??"",User.FindFirstValue("rol")??""));
}
