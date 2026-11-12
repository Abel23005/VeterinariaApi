using AutoMapper;
using Veterinaria.Entidades.DTOs;
using Veterinaria.Entidades.Models;
namespace Veterinaria.Negocio.Mapping;
public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Dueno, DuenoDto>()
            .ForCtorParam(nameof(DuenoDto.CantidadMascotas), o => o.MapFrom(s => s.Mascotas.Count))
            .ForMember(d => d.CantidadMascotas, o => o.MapFrom(s => s.Mascotas.Count));
        CreateMap<DuenoCrearDto, Dueno>();
        CreateMap<DuenoActualizarDto, Dueno>()
            .ForMember(d => d.DuenoId, o => o.Ignore())
            .ForMember(d => d.Mascotas, o => o.Ignore());
        CreateMap<Mascota, MascotaDto>()
            .ForCtorParam(nameof(MascotaDto.NombreDueno), o => o.MapFrom(s => s.Dueno.Nombre))
            .ForMember(d => d.NombreDueno, o => o.MapFrom(s => s.Dueno.Nombre));
        CreateMap<MascotaCrearDto, Mascota>()
            .ForMember(d => d.MascotaId, o => o.Ignore())
            .ForMember(d => d.Dueno, o => o.Ignore());
        CreateMap<MascotaActualizarDto, Mascota>()
            .ForMember(d => d.MascotaId, o => o.Ignore())
            .ForMember(d => d.Dueno, o => o.Ignore());
    }
}
