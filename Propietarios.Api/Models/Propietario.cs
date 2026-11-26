namespace Propietarios.Api.Models;

public sealed record Propietario(int Id, string Nombre, string Correo, string Telefono);

public interface IPropietarioStore
{
    IReadOnlyList<Propietario> ObtenerTodos();
    Propietario? ObtenerPorId(int id);
}

public sealed class InMemoryPropietarioStore : IPropietarioStore
{
    private readonly IReadOnlyList<Propietario> propietarios =
    [
        new(1, "Ana Torres", "ana@example.com", "999111222"),
        new(2, "Luis Pérez", "luis@example.com", "999333444"),
        new(3, "María Gómez", "maria@example.com", "999555666")
    ];

    public IReadOnlyList<Propietario> ObtenerTodos() => propietarios;

    public Propietario? ObtenerPorId(int id) => propietarios.FirstOrDefault(item => item.Id == id);
}
