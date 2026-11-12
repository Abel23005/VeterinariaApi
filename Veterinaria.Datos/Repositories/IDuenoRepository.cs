using Veterinaria.Entidades.Models;
namespace Veterinaria.Datos.Repositories;
public interface IDuenoRepository
{
    Task<List<Dueno>> ObtenerTodosAsync(CancellationToken ct = default);
    Task<Dueno?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<Dueno?> ObtenerPorCorreoAsync(string correo, CancellationToken ct = default);
    Task AgregarAsync(Dueno dueno, CancellationToken ct = default);
    void Eliminar(Dueno dueno);
    Task GuardarCambiosAsync(CancellationToken ct = default);
}
