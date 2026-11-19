using Microsoft.EntityFrameworkCore;
using Veterinaria.Datos.Data;
using Veterinaria.Entidades.Models;
namespace Veterinaria.Datos.Repositories;
public sealed class UsuarioRepository(VeterinariaContext db) : IUsuarioRepository {
 public Task<Usuario?> ObtenerPorCorreoAsync(string correo,CancellationToken ct=default)=>db.Usuarios.SingleOrDefaultAsync(x=>x.Correo==correo,ct);
 public async Task AgregarAsync(Usuario usuario,CancellationToken ct=default)=>await db.Usuarios.AddAsync(usuario,ct);
 public Task GuardarCambiosAsync(CancellationToken ct=default)=>db.SaveChangesAsync(ct);
}
