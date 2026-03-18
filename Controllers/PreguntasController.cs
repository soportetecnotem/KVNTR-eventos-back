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
    public class PreguntasController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public PreguntasController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        //Seccion Preguntas
        [HttpPost("Nueva")]
        [EndpointSummary("Crea una pregunta")]
        public async Task<ActionResult<RespuestaObjetoDTO>> CreatePregunta(PreguntaDTO model)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };
            if (model.Texto == "")
            {
                respuesta.Message.Add("La pregunta no puede estar vacia.");
                return respuesta;
            }

            var pregunta = mapper.Map<Pregunta>(model);

            context.Add(pregunta);
            await context.SaveChangesAsync();

            model.Id = pregunta.Id;
            respuesta.Status = true;
            respuesta.Message.Add("Pregunta creada.");
            respuesta.Response = model;

            return respuesta;
        }

        [HttpPut("Actualizar/{id:int}")]
        [EndpointSummary("Actualiza una pregunta por Id")]
        public async Task<ActionResult<RespuestaObjetoDTO>> ActualizarPregunta(int id, PreguntaDTO model)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var pregunta = mapper.Map<Pregunta>(model);
            pregunta.Id = id;
            context.Update(pregunta);
            await context.SaveChangesAsync();

            var editado = mapper.Map<PreguntaDTO>(pregunta);

            respuesta.Status = true;
            respuesta.Message.Add("Pregunta actualizada.");
            respuesta.Response = editado;

            return respuesta;
        }

        [HttpGet("Detalles/{id:int}")]
        [EndpointSummary("Obtiene los detalles de una pregunta. Buscado por ID")]
        public async Task<ActionResult<RespuestaObjetoDTO>> PreguntaXId(int id)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var pregunta = await context.Preguntas
                .FirstOrDefaultAsync(x => x.Id == id);

            if (pregunta is null)
            {
                respuesta.Message.Add("Pregunta no encontrado.");
                return respuesta;
            }
            else
            {
                var PreguntaDTO = mapper.Map<PreguntaDTO>(pregunta);
                respuesta.Status = true;
                respuesta.Message.Add("Ok");
                respuesta.Response = PreguntaDTO;
            }

            return respuesta;
        }

        [HttpDelete("{id:int}")]
        [EndpointSummary("Elimina una pregunta por Id")]
        public async Task<ActionResult<RespuestaObjetoDTO>> DeletePregunta(int id)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var registroABorrar = await context.Preguntas.Where(x => x.Id == id).ExecuteDeleteAsync();

            if (registroABorrar == 0)
            {
                respuesta.Message.Add("Pregunta No Encontrada.");
                return respuesta;
            }
            else
            {
                respuesta.Status = true;
                respuesta.Message.Add("Pregunta Eliminada.");
                var BorradoInfo = "Pregunta Id : " + id;
                respuesta.Response = BorradoInfo;
            }
            return respuesta;
        }

        [HttpGet("ListadoPreguntas")]
        [EndpointSummary("Obtiene la lista de todas las preguntas")]
        public async Task<ActionResult<RespuestaObjetoDTO>> GetPreguntas()
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var preguntas = await context.Preguntas.ToListAsync();
            var preguntasDTO = mapper.Map<IEnumerable<PreguntaDTO>>(preguntas);
            respuesta.Status = true;
            respuesta.Message.Add(preguntasDTO.Count() + " preguntas.");
            respuesta.Response = preguntasDTO;

            return respuesta;
        }

        [HttpGet("evento/{eventoId}/preguntas-respuestas")]
        [EndpointSummary("Obtienen todas las preguntas con sus respuestas de un Evento por Id")]
        public async Task<ActionResult<List<PreguntaConRespuestasDTO>>> ObtenerPreguntasConTodasLasRespuestas(int eventoId)
        {
            var preguntas = await context.Preguntas
                .Include(p => p.Respuestas
                    .Where(r => r.EventoId == eventoId))
                .ThenInclude(r => r.Convencionista)
                .ToListAsync();

            var resultado = preguntas.Select(p => new PreguntaConRespuestasDTO
            {
                PreguntaId = p.Id,
                Texto = p.Texto,
                Respuestas = [.. p.Respuestas.Select(r => new RespuestaDTO
                {
                    PreguntaId = p.Id,
                    EventoId = r.EventoId,
                    ConvencionistaId = r.ConvencionistaId,
                    Comentario = r.Comentario,
                    ClaveConvencionista = r.Convencionista!.Clave,
                    NombreConvencionista = r.Convencionista.NombreCompleto,
                    Calificacion = r.Calificacion
                })]
            }).ToList();

            return Ok(resultado);
        }

    }
}