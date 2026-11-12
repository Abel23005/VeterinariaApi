using Veterinaria.Entidades.Models;
namespace Veterinaria.Datos.Repositories;
public interface IMascotaRepository
{
    Task<List<Mascota>> ObtenerAsync(string? especie, int? duenoId, CancellationToken ct = default);
    Task<Mascota?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<bool> ExisteDuenoAsync(int duenoId, CancellationToken ct = default);
    Task AgregarAsync(Mascota mascota, CancellationToken ct = default);
    void Eliminar(Mascota mascota);
    Task GuardarCambiosAsync(CancellationToken ct = default);
}
