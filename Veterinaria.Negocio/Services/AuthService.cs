using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Veterinaria.Datos.Repositories;
using Veterinaria.Entidades.DTOs;
using Veterinaria.Entidades.Models;
using Veterinaria.Negocio.Exceptions;
namespace Veterinaria.Negocio.Services;
public sealed class AuthService(IUsuarioRepository repo,IConfiguration config):IAuthService {
 public async Task<TokenDto> RegistrarAsync(RegistroDto dto,CancellationToken ct=default){var correo=dto.Correo.Trim().ToLowerInvariant();if(await repo.ObtenerPorCorreoAsync(correo,ct)is not null)throw new ReglaNegocioException("El correo ya está registrado.");var u=new Usuario{Correo=correo,ClaveHash=BCrypt.Net.BCrypt.HashPassword(dto.Clave),Rol="Recepcionista"};await repo.AgregarAsync(u,ct);await repo.GuardarCambiosAsync(ct);return Crear(u);}
 public async Task<TokenDto?> LoginAsync(LoginDto dto,CancellationToken ct=default){var u=await repo.ObtenerPorCorreoAsync(dto.Correo.Trim().ToLowerInvariant(),ct);return u is null||!BCrypt.Net.BCrypt.Verify(dto.Clave,u.ClaveHash)?null:Crear(u);}
 private TokenDto Crear(Usuario u){var s=config.GetSection("Jwt");var minutos=int.Parse(s["MinutosVida"]!);var exp=DateTime.UtcNow.AddMinutes(minutos);var claims=new[]{new Claim(JwtRegisteredClaimNames.Sub,u.UsuarioId.ToString()),new Claim(JwtRegisteredClaimNames.Email,u.Correo),new Claim("rol",u.Rol)};var key=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(s["Clave"]!));var token=new JwtSecurityToken(s["Emisor"],s["Audiencia"],claims,expires:exp,signingCredentials:new SigningCredentials(key,SecurityAlgorithms.HmacSha256));return new TokenDto(new JwtSecurityTokenHandler().WriteToken(token),exp);}
}
