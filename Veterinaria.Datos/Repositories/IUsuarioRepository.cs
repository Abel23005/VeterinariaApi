using Veterinaria.Entidades.Models;
namespace Veterinaria.Datos.Repositories;
public interface IUsuarioRepository { Task<Usuario?> ObtenerPorCorreoAsync(string correo, CancellationToken ct=default); Task AgregarAsync(Usuario usuario, CancellationToken ct=default); Task GuardarCambiosAsync(CancellationToken ct=default); }
