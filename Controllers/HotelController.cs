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
    public class HotelController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public HotelController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpPost("Nuevo")]
        [EndpointSummary("Crea un nuevo Hotel")]
        public async Task<ActionResult<RespuestaObjetoDTO>> CreateHotel(HotelDTO model)
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

            if (model.NombreHotel == "")
            {
                respuesta.Message.Add("El nombre del Hotel no puede estar vacio.");
                return respuesta;
            }
            else
            {
                var hotel = mapper.Map<Hotel>(model);

                // Asociar usuarios si vienen en el modelo
                if (model.ConvencionistasIds?.Any() == true)
                {
                    hotel.Convencionistas = model.ConvencionistasIds.Select(cId => new HotelUsuario
                    {
                        ConvencionistaId = cId
                    }).ToList();
                }

                context.Add(hotel);
                await context.SaveChangesAsync();

                var hotelDTO = mapper.Map<DetalleHotelDTO>(hotel);

                respuesta.Status = true;
                respuesta.Message.Add("Hotel agregado");
                hotelDTO.Id = hotel.Id;
                respuesta.Response = hotelDTO;
            }
            return respuesta;
        }

        [HttpGet("Listado")]
        [EndpointSummary("Obtiene la lista de todos los hoteles")]
        public async Task<ActionResult<RespuestaObjetoDTO>> GetAllHotel()
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var hoteles = await context.Hoteles
                .Include(x => x.Evento)
                .ToListAsync();
            var hotelesDTO = mapper.Map<IEnumerable<ListadoHotelDTO>>(hoteles);
            respuesta.Status = true;
            respuesta.Message.Add(hotelesDTO.Count() + " hoteles.");
            respuesta.Response = hotelesDTO;

            return respuesta;
        }

        [HttpGet("ListadoXEvento/{IdEvento:int}")]
        [EndpointSummary("Obtiene la lista de todos los hoteles de un evento")]
        public async Task<ActionResult<RespuestaObjetoDTO>> GetHotelesXEvento(int IdEvento)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var hoteles = await context.Hoteles
                .Include(x => x.Evento)
                .Where(h => h.EventoId == IdEvento).ToListAsync();
            var hotelesDTO = mapper.Map<IEnumerable<ListadoHotelDTO>>(hoteles);
            respuesta.Status = true;
            respuesta.Message.Add(hotelesDTO.Count() + " hoteles.");
            respuesta.Response = hotelesDTO;

            return respuesta;
        }

        [HttpGet("Listado/{convencionistaId:int}/{eventoId:int}")]
        [EndpointSummary("Obtiene los hoteles de un convencionista en un evento específico")]
        public async Task<ActionResult<RespuestaObjetoDTO>> GetHotelesXConvencionista(int convencionistaId, int eventoId)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var hoteles = await context.Hoteles
                .Include(h => h.Evento)
                .Include(h => h.Convencionistas)
                    .ThenInclude(hu => hu.Convencionista)
                .Where(h => h.EventoId == eventoId &&
                            h.Convencionistas.Any(hu => hu.ConvencionistaId == convencionistaId))
                .ToListAsync();

            if (hoteles.Any())
            {
                var hotelesDTO = mapper.Map<IEnumerable<ListadoHotelDTO>>(hoteles);
                respuesta.Status = true;
                respuesta.Message.Add($"{hotelesDTO.Count()} hoteles encontrados para el convencionista en el evento {eventoId}.");
                respuesta.Response = hotelesDTO;
            }
            else
            {
                respuesta.Status = false;
                respuesta.Message.Add("No se encontraron hoteles para este convencionista en el evento especificado.");
            }

            return respuesta;
        }

        [HttpGet("Detalles/{id:int}")]
        [EndpointSummary("Obtiene los detalles de un hotel. Solicita por Id.")]
        public async Task<ActionResult<RespuestaObjetoDTO>> GetHotelId(int id)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var hotel = await context.Hoteles
                .Include(x => x.Evento)
                .Include(x => x.Convencionistas)
                .ThenInclude(h => h.Convencionista)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (hotel is null)
            {
                respuesta.Message.Add("Hotel no encontrado");
                return respuesta;
            }
            else
            {
                var hotelDTO = mapper.Map<DetalleHotelDTO>(hotel);
                respuesta.Status = true;
                respuesta.Message.Add("Ok");
                respuesta.Response = hotelDTO;
            }

            return respuesta;
        }

        [HttpPut("Actualizar/{id:int}")]
        [EndpointSummary("Actualiza un Hotel por Id")]
        public async Task<ActionResult<RespuestaObjetoDTO>> ActualizarHotel(int id, HotelDTO model)
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

            var hotel = await context.Hoteles
                .Include(a => a.Convencionistas)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (hotel == null)
            {
                respuesta.Message.Add("El hotel no fue encontrado.");
                respuesta.Response = id;
                return respuesta;
            }

            mapper.Map(model, hotel);

            // Asociar usuarios si vienen en el modelo
            // Verificar que todos los IDs recibidos existen
            var idsValidos = await context.Convencionistas
                .Where(c => model.ConvencionistasIds.Contains(c.Id))
                .Select(c => c.Id)
                .ToListAsync();

            var idsRecibidos = model.ConvencionistasIds ?? [];

            // Eliminar relaciones que ya no están en el modelo
            hotel.Convencionistas.RemoveAll(r => !idsRecibidos.Contains(r.ConvencionistaId));

            // Agregar nuevas relaciones si no existen
            var idsRelacionados = hotel.Convencionistas.Select(r => r.ConvencionistaId).ToList();

            var nuevosIds = idsValidos.Except(idsRelacionados);

            foreach (var nuevoId in nuevosIds)
            {
                hotel.Convencionistas.Add(new HotelUsuario
                {
                    ConvencionistaId = nuevoId
                });
            }

            await context.SaveChangesAsync();

            var editado = mapper.Map<DetalleHotelDTO>(hotel);

            respuesta.Status = true;
            respuesta.Message.Add("Hotel actualizado");
            respuesta.Response = editado;

            return respuesta;
        }

        [HttpDelete("{id:int}")]
        [EndpointSummary("Elimina un hotel por Id")]
        public async Task<ActionResult<RespuestaObjetoDTO>> DeleteHotel(int id)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var registroABorrar = await context.Hoteles
                .Include(x => x.Convencionistas)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (registroABorrar == null)
            {
                respuesta.Message.Add("Hotel No Encontrado.");
                respuesta.Response = $"Hotel Id: {id}";
                return respuesta;
            }

            // Eliminar relaciones hijas primero 
            context.RemoveRange(registroABorrar.Convencionistas);

            // Luego eliminar el hotel
            context.Hoteles.Remove(registroABorrar);
            await context.SaveChangesAsync();

            respuesta.Status = true;
            respuesta.Message.Add("Hotel Eliminado");
            respuesta.Response = $"Hotel Id: {id}";

            return respuesta;
        }

    }
}
