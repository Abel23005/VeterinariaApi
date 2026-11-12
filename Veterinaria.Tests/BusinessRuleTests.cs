using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Veterinaria.Datos.Repositories;
using Veterinaria.Entidades.DTOs;
using Veterinaria.Entidades.Models;
using Veterinaria.Negocio.Exceptions;
using Veterinaria.Negocio.Mapping;
using Veterinaria.Negocio.Services;

namespace Veterinaria.Tests;

public sealed class BusinessRuleTests
{
    [Fact]
    public async Task OwnerCannotBeCreatedWithDuplicateEmail()
    {
        var repository = new Mock<IDuenoRepository>();
        repository.Setup(r => r.ObtenerPorCorreoAsync("ana@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dueno { DuenoId = 1, Nombre = "Ana", Correo = "ana@example.com", Telefono = "999111222" });
        var service = new DuenoService(repository.Object, Mock.Of<IMapper>());

        await Assert.ThrowsAsync<ReglaNegocioException>(() => service.CrearAsync(new DuenoCrearDto
        {
            Nombre = "Otra Ana", Correo = "ana@example.com", Telefono = "999000111"
        }));
        repository.Verify(r => r.AgregarAsync(It.IsAny<Dueno>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task OwnerWithPetsCannotBeDeleted()
    {
        var owner = new Dueno
        {
            DuenoId = 1, Nombre = "Ana", Correo = "ana@example.com", Telefono = "999111222",
            Mascotas = [new Mascota { MascotaId = 1, Nombre = "Luna" }]
        };
        var repository = new Mock<IDuenoRepository>();
        repository.Setup(r => r.ObtenerPorIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(owner);
        var service = new DuenoService(repository.Object, Mock.Of<IMapper>());

        await Assert.ThrowsAsync<ReglaNegocioException>(() => service.EliminarAsync(1));
        repository.Verify(r => r.Eliminar(It.IsAny<Dueno>()), Times.Never);
    }

    [Fact]
    public async Task PetCannotHaveFutureBirthDate()
    {
        var repository = new Mock<IMascotaRepository>();
        var service = new MascotaService(repository.Object, Mock.Of<IMapper>());

        await Assert.ThrowsAsync<ReglaNegocioException>(() => service.CrearAsync(new MascotaCrearDto
        {
            Nombre = "Luna", Especie = "Perro", Raza = "Labrador",
            FechaNacimiento = DateOnly.FromDateTime(DateTime.Today.AddDays(1)), DuenoId = 1
        }));
        repository.Verify(r => r.AgregarAsync(It.IsAny<Mascota>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task PetCannotReferenceMissingOwner()
    {
        var repository = new Mock<IMascotaRepository>();
        repository.Setup(r => r.ExisteDuenoAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var service = new MascotaService(repository.Object, Mock.Of<IMapper>());

        await Assert.ThrowsAsync<ReglaNegocioException>(() => service.CrearAsync(new MascotaCrearDto
        {
            Nombre = "Luna", Especie = "Perro", Raza = "Labrador",
            FechaNacimiento = DateOnly.FromDateTime(DateTime.Today.AddYears(-2)), DuenoId = 999
        }));
        repository.Verify(r => r.AgregarAsync(It.IsAny<Mascota>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void MappingProfileFlattensOwnerNameIntoPetDto()
    {
        var configuration = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), NullLoggerFactory.Instance);
        var mapper = configuration.CreateMapper();
        var result = mapper.Map<MascotaDto>(new Mascota
        {
            MascotaId = 4, Nombre = "Luna", Especie = "Perro", Raza = "Labrador",
            FechaNacimiento = new DateOnly(2021, 4, 12), DuenoId = 2,
            Dueno = new Dueno { DuenoId = 2, Nombre = "Ana Torres", Correo = "ana@example.com", Telefono = "999111222" }
        });

        Assert.Equal("Ana Torres", result.NombreDueno);
        Assert.Equal(2, result.DuenoId);
    }
}
