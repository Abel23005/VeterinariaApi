using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Veterinaria.Datos.Data;
using Veterinaria.Entidades.Models;

namespace Veterinaria.Tests;

public sealed class TestApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public TestApiFactory() => _connection.Open();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<VeterinariaContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<VeterinariaContext>>();
            services.AddDbContext<VeterinariaContext>(options => options.UseSqlite(_connection));
        });
    }

    public HttpClient CreateClientWithSeedData()
    {
        var client = CreateClient();
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<VeterinariaContext>();
        db.Database.EnsureCreated();
        db.Duenos.AddRange(
            new Dueno { DuenoId = 1, Nombre = "Ana Torres", Correo = "ana@example.com", Telefono = "999111222" },
            new Dueno { DuenoId = 2, Nombre = "Luis Perez", Correo = "luis@example.com", Telefono = "999333444" });
        db.Mascotas.Add(new Mascota
        {
            MascotaId = 1, Nombre = "Luna", Especie = "Perro", Raza = "Labrador",
            FechaNacimiento = new DateOnly(2021, 4, 12), DuenoId = 1
        });
        db.SaveChanges();
        return client;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _connection.Dispose();
    }
}
