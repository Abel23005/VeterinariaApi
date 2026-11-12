using Microsoft.EntityFrameworkCore;
using Veterinaria.Datos.Data;
using Veterinaria.Entidades.Models;
namespace Veterinaria.Datos.Repositories;
public sealed class DuenoRepository(VeterinariaContext db) : IDuenoRepository
{
    public Task<List<Dueno>> ObtenerTodosAsync(CancellationToken ct = default) => db.Duenos.Include(d => d.Mascotas).OrderBy(d => d.Nombre).ToListAsync(ct);
    public Task<Dueno?> ObtenerPorIdAsync(int id, CancellationToken ct = default) => db.Duenos.Include(d => d.Mascotas).FirstOrDefaultAsync(d => d.DuenoId == id, ct);
    public Task<Dueno?> ObtenerPorCorreoAsync(string correo, CancellationToken ct = default) => db.Duenos.FirstOrDefaultAsync(d => d.Correo == correo, ct);
    public async Task AgregarAsync(Dueno dueno, CancellationToken ct = default) => await db.Duenos.AddAsync(dueno, ct);
    public void Eliminar(Dueno dueno) => db.Duenos.Remove(dueno);
    public Task GuardarCambiosAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
