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
    public class EventosController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        public EventosController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet("debug/claims")]
        [AllowAnonymous]
        [EndpointSummary("Obtiene la lista de todos los claims del usuario, solo es endpoint de prueba.")]
        public IActionResult VerClaims()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value });
            return Ok(claims);
        }

        [HttpPost("Nuevo")]
        [EndpointSummary("Crea un nuevo Evento")]
        public async Task<ActionResult<RespuestaObjetoDTO>> CreateEvento(EventoDTO model)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };
            if (model.NombreEvento == "")
            {
                respuesta.Message.Add("El nombre del evento no puede estar vacio.");
                return respuesta;
            }
            else
            {
                var evento = mapper.Map<Evento>(model);
                evento.Activo = true;
                context.Add(evento);
                await context.SaveChangesAsync();

                respuesta.Status = true;
                respuesta.Message.Add("Evento creado");
                model.Id = evento.Id;
                respuesta.Response = model;
            }

            return respuesta;
        }

        [HttpGet("Listado")]
        [EndpointSummary("Obtiene la lista de todos los eventos")]
        public async Task<ActionResult<RespuestaObjetoDTO>> GetEventos()
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var eventos = await context.Eventos
                .ToListAsync();
            var eventosDTO = mapper.Map<IEnumerable<EventoDTO>>(eventos);
            respuesta.Status = true;
            respuesta.Message.Add(eventosDTO.Count() + " eventos");
            respuesta.Response = eventosDTO;

            return respuesta;
        }

        [HttpGet("ListadoPorConv")]
        [EndpointSummary("Obtiene la lista de todos los eventos con filtros opcionales de categoria de convencionista. Ejemplo: Por convencionista:GET /Listado?convencionistaId=10 - Por categoría: GET /Listado?categoriaUsuarioId=3 - Por evento específico: GET /Listado?eventoId=5 - Combinados: GET /Listado?convencionistaId=10&categoriaUsuarioId=3")]
        public async Task<ActionResult<RespuestaObjetoDTO>> GetEventos(
                    [FromQuery] int? convencionistaId,
                    [FromQuery] int? categoriaUsuarioId,
                    [FromQuery] int? eventoId)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var query = context.Eventos
                .Include(e => e.Convencionistas)
                    .ThenInclude(eu => eu.Convencionista)
                .AsQueryable();

            if (eventoId.HasValue && eventoId.Value > 0)
            {
                query = query.Where(e => e.Id == eventoId.Value);
            }

            if (convencionistaId.HasValue && convencionistaId.Value > 0)
            {
                query = query.Where(e => e.Convencionistas.Any(eu => eu.ConvencionistaId == convencionistaId.Value));
            }

            if (categoriaUsuarioId.HasValue && categoriaUsuarioId.Value > 0)
            {
                query = query.Where(e =>
                    e.Convencionistas.Any(eu => eu.Convencionista != null && eu.Convencionista.CategoriaUsuarioId == categoriaUsuarioId.Value));
            }

            var eventos = await query.ToListAsync();
            var eventosDTO = mapper.Map<IEnumerable<EventoDTO>>(eventos);

            respuesta.Status = true;
            respuesta.Message.Add($"{eventosDTO.Count()} eventos encontrados.");
            respuesta.Response = eventosDTO;

            return respuesta;
        }

        [HttpGet("Detalles/{id:int}")]
        [EndpointSummary("Obtiene los detalles de un evento. Solicita por Id.")]
        public async Task<ActionResult<RespuestaObjetoDTO>> GetEventoId(int id)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var evento = await context.Eventos.FirstOrDefaultAsync(x => x.Id == id);

            if (evento is null)
            {
                respuesta.Message.Add("Evento no encontrado");
                return respuesta;
            }
            else
            {
                var eventoDTO = mapper.Map<EventoDTO>(evento);
                respuesta.Status = true;
                respuesta.Message.Add("Ok");
                respuesta.Response = eventoDTO;
            }

            return respuesta;
        }

        [HttpPut("Actualizar/{id:int}")]
        [EndpointSummary("Actualiza un Evento por Id")]
        public async Task<ActionResult<RespuestaObjetoDTO>> ActualizarEvento(int id, EventoDTO model)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var evento = mapper.Map<Evento>(model);
            evento.Id = id;
            context.Update(evento);
            await context.SaveChangesAsync();

            var editado = mapper.Map<EventoDTO>(evento);

            respuesta.Status = true;
            respuesta.Message.Add("Evento actualizado");
            respuesta.Response = editado;

            return respuesta;
        }

        [HttpDelete("{id:int}")]
        [EndpointSummary("Elimina un Evento por Id")]
        //[HttpDelete("EliminarEvento/{id:int}")]
        public async Task<ActionResult<RespuestaObjetoDTO>> DeleteEvento(int id)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var evento = await context.Eventos.FirstOrDefaultAsync(e => e.Id == id);

            if (evento == null)
            {
                respuesta.Message.Add("Evento no encontrado.");
                return respuesta;
            }

            try
            {
                context.Eventos.Remove(evento);
                await context.SaveChangesAsync();

                respuesta.Status = true;
                respuesta.Message.Add("Evento eliminado correctamente.");
                respuesta.Response = $"Evento Id: {id}";
            }
            catch (Exception ex)
            {
                if (ex.InnerException!.Message.Contains("FK"))
                {
                    respuesta.Message.Add("El evento tiene datos vinculados. Eliminación no permitida.");
                }
                else
                {
                    respuesta.Message.Add(ex.Message);
                }
                return respuesta;
            }

            return Ok(respuesta);
        }
    }
}