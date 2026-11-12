using Microsoft.EntityFrameworkCore;
using Veterinaria.Datos.Data;
using Veterinaria.Entidades.Models;
namespace Veterinaria.Datos.Repositories;
public sealed class MascotaRepository(VeterinariaContext db) : IMascotaRepository
{
    public Task<List<Mascota>> ObtenerAsync(string? especie, int? duenoId, CancellationToken ct = default)
    {
        var query = db.Mascotas.Include(m => m.Dueno).AsQueryable();
        if (!string.IsNullOrWhiteSpace(especie)) query = query.Where(m => m.Especie == especie);
        if (duenoId.HasValue) query = query.Where(m => m.DuenoId == duenoId.Value);
        return query.OrderBy(m => m.Nombre).ToListAsync(ct);
    }
    public Task<Mascota?> ObtenerPorIdAsync(int id, CancellationToken ct = default) => db.Mascotas.Include(m => m.Dueno).FirstOrDefaultAsync(m => m.MascotaId == id, ct);
    public Task<bool> ExisteDuenoAsync(int duenoId, CancellationToken ct = default) => db.Duenos.AnyAsync(d => d.DuenoId == duenoId, ct);
    public async Task AgregarAsync(Mascota mascota, CancellationToken ct = default) => await db.Mascotas.AddAsync(mascota, ct);
    public void Eliminar(Mascota mascota) => db.Mascotas.Remove(mascota);
    public Task GuardarCambiosAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
