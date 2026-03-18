using AutoMapper;
using EventosBack.Data;
using EventosBack.DTO;
using EventosBack.DTO.Responses;
using EventosBack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventosBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VuelosController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public VuelosController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpPost("Nuevo")]
        [EndpointSummary("Crea un nuevo Vuelo")]
        public async Task<ActionResult<RespuestaObjetoDTO>> CreateVuelo(ActualizarVueloDTO model)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };
            // Validar que los IDs no vengan vacíos o inválidos
            if (model.EventoId <= 0)
            {
                respuesta.Message.Add("Debe proporcionar un IdEvento válido.");
                respuesta.Response = model.EventoId;
                return respuesta;
            }

            // Validar existencia de claves foráneas en la base
            var eventoExiste = await context.Eventos
                .AnyAsync(c => c.Id == model.EventoId);

            if (!eventoExiste)
            {
                respuesta.Message.Add("El evento seleccionado no existe.");
                respuesta.Response = model.EventoId;
                return respuesta;
            }
            var vuelo = mapper.Map<Vuelo>(model);

            // Asociar usuarios si vienen en el modelo
            if (model.ConvencionistasIds?.Any() == true)
            {
                vuelo.VuelosUsuarios = model.ConvencionistasIds.Select(cId => new VueloUsuario
                {
                    ConvencionistaId = cId
                }).ToList();
            }

            context.Add(vuelo);
            await context.SaveChangesAsync();

            var VueloDTO = mapper.Map<DetalleVuelosDTO>(vuelo);

            respuesta.Status = true;
            respuesta.Message.Add("Vuelo agregado.");
            respuesta.Response = VueloDTO;

            return respuesta;
        }

        [HttpGet("Listado")]
        [EndpointSummary("Obtiene un listado de vuelos")]
        public async Task<ActionResult<RespuestaObjetoDTO>> GetVuelos()
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var Vuelos = await context.Vuelos
                .Include(x => x.Evento)
                .ToListAsync();
            var ConvDTO = mapper.Map<IEnumerable<ListaVuelosDTO>>(Vuelos);

            respuesta.Status = true;
            respuesta.Message.Add(ConvDTO.Count() + " vuelos encontrados");
            respuesta.Response = ConvDTO;

            return respuesta;
        }

        [HttpGet("Listado/{convencionistaId:int}/{eventoId:int}")]
        [EndpointSummary("Obtiene los vuelos de un convencionista en un evento específico")]
        public async Task<ActionResult<RespuestaObjetoDTO>> GetVuelosXConvencionista(int convencionistaId, int eventoId)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var vuelos = await context.Vuelos
                .Include(v => v.Evento)
                .Include(v => v.VuelosUsuarios)
                    .ThenInclude(vu => vu.Convencionista)
                .Where(v => v.EventoId == eventoId &&
                            v.VuelosUsuarios.Any(vu => vu.ConvencionistaId == convencionistaId))
                .OrderBy(v => v.Fecha_Vuelo) // opcional: ordena por vuelo más próximo primero
                .ToListAsync();

            if (vuelos.Any())
            {
                var vuelosDTO = mapper.Map<IEnumerable<ListaVuelosDTO>>(vuelos);
                respuesta.Status = true;
                respuesta.Message.Add($"{vuelosDTO.Count()} vuelos encontrados para el convencionista en el evento {eventoId}.");
                respuesta.Response = vuelosDTO;
            }
            else
            {
                respuesta.Status = false;
                respuesta.Message.Add("No se encontraron vuelos para este convencionista en el evento especificado.");
            }

            return respuesta;
        }

        [HttpGet("Detalles/{id:int}")]
        [EndpointSummary("Obtiene los detalles de un Vuelo. Buscado por ID")]
        public async Task<ActionResult<RespuestaObjetoDTO>> VueloXId(int id)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var vuelo = await context.Vuelos
                .Include(x => x.Evento)
                .Include(x => x.VuelosUsuarios)
                .ThenInclude(vu => vu.Convencionista)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (vuelo is null)
            {
                respuesta.Message.Add("Vuelo no encontrado.");
                return respuesta;
            }
            else
            {
                var VueloDTO = mapper.Map<DetalleVuelosDTO>(vuelo);
                respuesta.Status = true;
                respuesta.Message.Add("Ok");
                respuesta.Response = VueloDTO;
            }

            return respuesta;
        }

        [HttpPut("Actualizar/{id:int}")]
        [EndpointSummary("Actualiza un Vuelo por Id")]
        public async Task<ActionResult<RespuestaObjetoDTO>> ActualizarVuelo(int id, ActualizarVueloDTO model)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            // Validar que los IDs no vengan vacíos o inválidos
            if (model.EventoId <= 0)
            {
                respuesta.Message.Add("Debe proporcionar un IdEvento válido.");
                respuesta.Response = model.EventoId;
                return respuesta;
            }

            // Validar existencia de claves foráneas en la base
            var eventoExiste = await context.Eventos
                .AnyAsync(c => c.Id == model.EventoId);

            if (!eventoExiste)
            {
                respuesta.Message.Add("El evento seleccionado no existe.");
                respuesta.Response = model.EventoId;
                return respuesta;
            }
            var vuelo = await context.Vuelos
                .Include(a => a.VuelosUsuarios)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (vuelo == null)
            {
                respuesta.Message.Add("El vuelo no fue encontrado.");
                respuesta.Response = id;
                return respuesta;
            }

            mapper.Map(model, vuelo);

            // Asociar usuarios si vienen en el modelo
            // Verificar que todos los IDs recibidos existen
            var idsValidos = await context.Convencionistas
                .Where(c => model.ConvencionistasIds.Contains(c.Id))
                .Select(c => c.Id)
                .ToListAsync();

            var idsRecibidos = model.ConvencionistasIds ?? [];

            // Eliminar relaciones que ya no están en el modelo
            vuelo.VuelosUsuarios.RemoveAll(r => !idsRecibidos.Contains(r.ConvencionistaId));

            // Agregar nuevas relaciones si no existen
            var idsRelacionados = vuelo.VuelosUsuarios.Select(r => r.ConvencionistaId).ToList();

            var nuevosIds = idsValidos.Except(idsRelacionados);

            foreach (var nuevoId in nuevosIds)
            {
                vuelo.VuelosUsuarios.Add(new VueloUsuario
                {
                    ConvencionistaId = nuevoId
                });
            }

            await context.SaveChangesAsync();

            var editado = mapper.Map<DetalleVuelosDTO>(vuelo);

            respuesta.Status = true;
            respuesta.Message.Add("Vuelo actualizado.");
            respuesta.Response = editado;

            return respuesta;
        }

        [HttpDelete("{id:int}")]
        [EndpointSummary("Elimina un vuelo por Id")]
        public async Task<ActionResult<RespuestaObjetoDTO>> DeleteVuelo(int id)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var vueloABorrar = await context.Vuelos
                .Include(a => a.VuelosUsuarios)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (vueloABorrar == null)
            {
                respuesta.Message.Add("Vuelo no encontrada.");
                respuesta.Response = $"Vuelo Id: {id}";
                return respuesta;
            }

            // Eliminar relaciones hijas primero (VueloUsuario)
            context.RemoveRange(vueloABorrar.VuelosUsuarios);

            // Luego eliminar el vuelo
            context.Vuelos.Remove(vueloABorrar);
            await context.SaveChangesAsync();

            respuesta.Status = true;
            respuesta.Message.Add("Vuelo Eliminado");
            respuesta.Response = $"Vuelo Id: {id}";

            return respuesta;
        }

        [HttpGet("PorConvencionista/{ConvencionistaId:int}/Evento/{EventoId:int}")]
        [EndpointSummary("Obtiene los vuelos asignados a un convencionista dentro de un evento")]
        public async Task<ActionResult<RespuestaObjetoDTO>> VuelosXConvencionista(int ConvencionistaId, int EventoId)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var Vuelos = await context.Vuelos
                .Include(x => x.Evento)
                .Include(x => x.VuelosUsuarios)
                .Where(v =>
                    v.EventoId == EventoId &&
                    v.VuelosUsuarios.Any(vu => vu.ConvencionistaId == ConvencionistaId)
                )
                .OrderBy(v => v.Fecha_Vuelo)
                .ToListAsync();

            var ConvDTO = mapper.Map<IEnumerable<ListaVuelosDTO>>(Vuelos);

            respuesta.Status = true;
            respuesta.Message.Add($"{ConvDTO.Count()} vuelos encontrados para el convencionista en el evento indicado.");
            respuesta.Response = ConvDTO;

            return respuesta;
        }

    }
}