using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Veterinaria.Tests;

public sealed class ApiResponseTests
{
    [Fact]
    public async Task GetOwnersReturns200()
    {
        using var factory = new TestApiFactory();
        using var client = factory.CreateClientWithSeedData();

        var response = await client.GetAsync("/api/duenos");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task OwnerPetAndFilterRoutesReturnDtos()
    {
        using var factory = new TestApiFactory();
        using var client = factory.CreateClientWithSeedData();

        var owner = await client.GetAsync("/api/duenos/1");
        var nested = await client.GetAsync("/api/duenos/1/mascotas");
        var filtered = await client.GetAsync("/api/mascotas?especie=Perro&duenoId=1");
        var pet = await client.GetAsync("/api/mascotas/1");
        var petBody = await pet.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, owner.StatusCode);
        Assert.Equal(HttpStatusCode.OK, nested.StatusCode);
        Assert.Equal(HttpStatusCode.OK, filtered.StatusCode);
        Assert.Equal(HttpStatusCode.OK, pet.StatusCode);
        Assert.Contains("Ana Torres", petBody);
    }

    [Fact]
    public async Task SwaggerListsDtoContractsWithoutEntityTypes()
    {
        using var factory = new TestApiFactory();
        using var client = factory.CreateClientWithSeedData();

        var response = await client.GetAsync("/swagger/v1/swagger.json");
        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);
        var schemas = document.RootElement.GetProperty("components").GetProperty("schemas")
            .EnumerateObject().Select(schema => schema.Name).ToArray();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("DuenoDto", schemas);
        Assert.Contains("MascotaDto", schemas);
        Assert.DoesNotContain("Dueno", schemas);
        Assert.DoesNotContain("Mascota", schemas);
    }

    [Fact]
    public async Task CreateOwnerReturns201AndDuplicateEmailReturns409()
    {
        using var factory = new TestApiFactory();
        using var client = factory.CreateClientWithSeedData();
        var dto = new { nombre = "Carla Diaz", correo = "carla@example.com", telefono = "999555666" };

        var created = await client.PostAsJsonAsync("/api/duenos", dto);
        var duplicate = await client.PostAsJsonAsync("/api/duenos", dto);

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        Assert.NotNull(created.Headers.Location);
    }

    [Fact]
    public async Task UpdateOwnerReturns204()
    {
        using var factory = new TestApiFactory();
        using var client = factory.CreateClientWithSeedData();

        var response = await client.PutAsJsonAsync("/api/duenos/2", new
        {
            nombre = "Luis Actualizado", correo = "luis.nuevo@example.com", telefono = "999333444"
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task CreatePetReturns201()
    {
        using var factory = new TestApiFactory();
        using var client = factory.CreateClientWithSeedData();

        var response = await client.PostAsJsonAsync("/api/mascotas", new
        {
            nombre = "Milo", especie = "Gato", raza = "Criollo", fechaNacimiento = "2022-08-03", duenoId = 1
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task InvalidOwnerReturns400AndMissingPetReturns404()
    {
        using var factory = new TestApiFactory();
        using var client = factory.CreateClientWithSeedData();

        var invalid = await client.PostAsJsonAsync("/api/duenos", new { nombre = "A", correo = "bad", telefono = "1" });
        var missing = await client.GetAsync("/api/mascotas/9999");

        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    [Fact]
    public async Task UpdateAndDeletePetReturn204AndOwnerWithPetsReturns409()
    {
        using var factory = new TestApiFactory();
        using var client = factory.CreateClientWithSeedData();
        var dto = new { nombre = "Luna Actualizada", especie = "Perro", raza = "Labrador", fechaNacimiento = "2021-04-12", duenoId = 1 };

        var updated = await client.PutAsJsonAsync("/api/mascotas/1", dto);
        var deleteOwner = await client.DeleteAsync("/api/duenos/1");
        var deleted = await client.DeleteAsync("/api/mascotas/1");

        Assert.Equal(HttpStatusCode.NoContent, updated.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, deleteOwner.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
    }
}
