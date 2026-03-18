using AutoMapper;
using EventosBack.DTO;
using EventosBack.Models;

namespace EventosBack.Utilidades
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Evento, EventoDTO>()
                .ForMember(dto => dto.NombreEvento, config => config.MapFrom(ent => ent.NombreConvencion))
                .ReverseMap();

            CreateMap<Usuario, UsuarioDTO>()
                .ForMember(dto => dto.FirstName, config => config.MapFrom(ent => ent.Nombre))
                .ForMember(dto => dto.LastName, config => config.MapFrom(ent => ent.Apellidos))
                .ReverseMap();

            CreateMap<Usuario, CreacionUsuarioDTO>()
                .ForMember(dto => dto.FirstName, config => config.MapFrom(ent => ent.Nombre))
                .ForMember(dto => dto.LastName, config => config.MapFrom(ent => ent.Apellidos))
                .ReverseMap();

            CreateMap<Usuario, CredencialesUsuarioDTO>()
                .ReverseMap();

            CreateMap<Convencionista, CreateConvDTO>()
                  .ForMember(dto => dto.PerfilId, config => config.MapFrom(ent => ent.PerfilConvencionistaId))
                  .ForMember(dto => dto.CategoriaId, config => config.MapFrom(ent => ent.CategoriaUsuarioId))
                .ReverseMap();

            CreateMap<Convencionista, DetalleConvDTO>()
                .ForMember(dto => dto.PerfilId, config => config.MapFrom(ent => ent.PerfilConvencionista!.Id))
                .ForMember(dto => dto.PerfilNombre, config => config.MapFrom(ent => ent.PerfilConvencionista!.Nombre))
                .ForMember(dto => dto.CategoriaId, config => config.MapFrom(ent => ent.CategoriaUsuario!.Id))
                .ForMember(dto => dto.CategoriaNombre, config => config.MapFrom(ent => ent.CategoriaUsuario!.Nombre))
                .ForMember(dto => dto.EventoId, config => config.MapFrom(ent =>
                        ent.EventosUsuarios != null && ent.EventosUsuarios.Any(eu => eu.Evento != null)
                            ? ent.EventosUsuarios
                            .Where(eu => eu.Activo && eu.Evento != null)
                            .Select(eu => eu.Evento!.Id)
                            .FirstOrDefault()
                            : 0
                ))
                .ForMember(dto => dto.NombreEvento, config => config.MapFrom(ent =>
                        ent.EventosUsuarios != null && ent.EventosUsuarios.Any(eu => eu.Evento != null)
                            ? ent.EventosUsuarios
                            .Where(eu => eu.Activo && eu.Evento != null)
                            .Select(eu => eu.Evento!.NombreConvencion)
                            .FirstOrDefault()
                            : null
                ))
               .ReverseMap();

            CreateMap<Hotel, DetalleHotelDTO>()
                 .ForMember(dest => dest.ConvencionistasIds,
                    opt => opt.MapFrom(src => src.Convencionistas.Select(vu => vu.ConvencionistaId)))
                .ForMember(dto => dto.NombreEvento, config => config.MapFrom(ent => ent.Evento!.NombreConvencion))
                .ReverseMap();

            CreateMap<Hotel, ListadoHotelDTO>()
                 .ForMember(dto => dto.NombreEvento, config => config.MapFrom(ent => ent.Evento!.NombreConvencion))
                .ReverseMap();

            CreateMap<Hotel, HotelDTO>()
                .ReverseMap();

            CreateMap<PerfilConvencionista, PerfilDTO>()
                .ReverseMap();

            CreateMap<CategoriaUsuario, CategoriaUDTO>()
                .ReverseMap();

            CreateMap<Pregunta, PreguntaDTO>()
                .ReverseMap();

            CreateMap<Recomendacion, CreateRecomendacionDTO>()
                .ReverseMap();

            CreateMap<Recomendacion, RecomendacionDTO>()
                 .ForMember(dto => dto.NombreEvento, config => config.MapFrom(ent => ent.Evento!.NombreConvencion))
                .ForMember(dto => dto.NombreCategoriaReco, config => config.MapFrom(ent => ent.CategoriaRecomendacion!.Nombre))
                 .ReverseMap();

            CreateMap<Categoria_Recomendacion, Categoria_RecomendacionDTO>()
                .ReverseMap();

            CreateMap<Categoria_Recomendacion, DetalleCatRecomendacionDTO>()
                .ReverseMap();

            CreateMap<Vuelo, VueloDTO>()
                .ReverseMap();

            CreateMap<Vuelo, DetalleVuelosDTO>()
                .ForMember(dest => dest.ConvencionistasIds,
                    opt => opt.MapFrom(src => src.VuelosUsuarios.Select(vu => vu.ConvencionistaId)))
                .ForMember(dest => dest.NombreEvento,
                    opt => opt.MapFrom(src => src.Evento!.NombreConvencion))
                .ReverseMap();

            CreateMap<Vuelo, ListaVuelosDTO>()
               .ForMember(dest => dest.NombreEvento,
                    opt => opt.MapFrom(src => src.Evento!.NombreConvencion))
                .ReverseMap();

            CreateMap<Vuelo, ActualizarVueloDTO>()
                .ReverseMap();

            CreateMap<Actividad, ActividadDTO>()
                .ReverseMap();

            CreateMap<Actividad, ActualizarActividadDTO>()
                .ReverseMap();

            CreateMap<Actividad, ListaActividadDTO>()
                 .ForMember(dto => dto.NombreEvento, config => config.MapFrom(ent => ent.Evento!.NombreConvencion))
                .ForMember(dto => dto.NombreCategoriaAc, config => config.MapFrom(ent => ent.Categoria!.Nombre))
                .ReverseMap();

            CreateMap<Actividad, DetallesActividadDTO>()
                .ForMember(dto => dto.NombreEvento, config => config.MapFrom(ent => ent.Evento!.NombreConvencion))
                .ForMember(dto => dto.NombreCategoriaAc, config => config.MapFrom(ent => ent.Categoria!.Nombre))
                 .ForMember(dest => dest.ConvencionistasIds,
                    opt => opt.MapFrom(src => src.Convencionistas.Select(vu => vu.ConvencionistaId)))
                .ReverseMap();

            CreateMap<Categoria_Actividades, Categoria_ActividadDTO>()
                .ReverseMap();

            CreateMap<DeviceInfoDTO, DeviceInfo>().ReverseMap();

            CreateMap<DetalleDeviceInfoDTO, DeviceInfo>().ReverseMap();

            CreateMap<VersionDTO, Version_App>().ReverseMap();

            CreateMap<DetalleVersionDTO, Version_App>().ReverseMap();

            CreateMap<ModuloDTO, Modulo>().ReverseMap();

            CreateMap<DetalleModuloDTO, Modulo>().ReverseMap();
        }
    }
}
