using Veterinaria.Entidades.DTOs;
namespace Veterinaria.Negocio.Services;
public interface IAuthService { Task<TokenDto> RegistrarAsync(RegistroDto dto,CancellationToken ct=default); Task<TokenDto?> LoginAsync(LoginDto dto,CancellationToken ct=default); }
