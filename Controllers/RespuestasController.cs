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
    public class RespuestasController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public RespuestasController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        //Seccion Respuestas
        [HttpPost("nueva")]
        public async Task<ActionResult<RespuestaObjetoDTO>> Responder([FromBody] CreacionRespuestaDTO respuestaDTO)
        {
            var response = new RespuestaObjetoDTO
            {
                Message = []
            };

            var yaRespondido = await context.Respuestas
                .AnyAsync(r => r.ConvencionistaId == respuestaDTO.ConvencionistaId && r.PreguntaId == respuestaDTO.PreguntaId && r.EventoId == respuestaDTO.EventoId);

            if (yaRespondido)
            {
                response.Message.Add("Ya has respondido esta pregunta.");
                return response;
            }

            var respuesta = new Respuesta
            {
                ConvencionistaId = respuestaDTO.ConvencionistaId,
                PreguntaId = respuestaDTO.PreguntaId,
                EventoId = respuestaDTO.EventoId,
                Comentario = respuestaDTO.Comentario,
                Calificacion = respuestaDTO.Calificacion
            };

            context.Respuestas.Add(respuesta);
            await context.SaveChangesAsync();

            response.Message.Add("Respuesta guardada.");
            response.Status = true;
            response.Response = respuestaDTO;
            return response;
        }

        [HttpPost("GuardarListado")]
        [EndpointSummary("Guarda un listado de respuestas")]
        public async Task<ActionResult<RespuestaObjetoDTO>> ResponderListado([FromBody] List<CreacionRespuestaDTO> respuestasDTO)
        {
            var response = new RespuestaObjetoDTO
            {
                Message = []
            };

            var respuestasAGuardar = new List<Respuesta>();

            foreach (var respuestaDTO in respuestasDTO)
            {
                var yaRespondido = await context.Respuestas
                    .AnyAsync(r => r.ConvencionistaId == respuestaDTO.ConvencionistaId
                                && r.PreguntaId == respuestaDTO.PreguntaId
                                && r.EventoId == respuestaDTO.EventoId);

                if (yaRespondido)
                {
                    response.Message.Add($"La pregunta {respuestaDTO.PreguntaId} ya fue respondida por el convencionista {respuestaDTO.ConvencionistaId} en el evento {respuestaDTO.EventoId}.");
                    continue; // salta esta respuesta y sigue con las demás
                }

                var respuesta = new Respuesta
                {
                    ConvencionistaId = respuestaDTO.ConvencionistaId,
                    PreguntaId = respuestaDTO.PreguntaId,
                    EventoId = respuestaDTO.EventoId,
                    Comentario = respuestaDTO.Comentario,
                    Calificacion = respuestaDTO.Calificacion
                };

                respuestasAGuardar.Add(respuesta);
            }

            if (respuestasAGuardar.Any())
            {
                context.Respuestas.AddRange(respuestasAGuardar);
                await context.SaveChangesAsync();

                response.Message.Add($"{respuestasAGuardar.Count} respuestas guardadas.");
                response.Status = true;
                response.Response = respuestasAGuardar;
            }
            else
            {
                response.Message.Add("No se guardaron nuevas respuestas.");
                response.Status = false;
            }

            return response;
        }

        [HttpDelete("{id:int}")]
        [EndpointSummary("Elimina una respuesta por Id")]
        public async Task<ActionResult<RespuestaObjetoDTO>> DeleteRespuesta(int id)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var registroABorrar = await context.Respuestas.Where(x => x.Id == id).ExecuteDeleteAsync();

            if (registroABorrar == 0)
            {
                respuesta.Message.Add("Respuesta No Encontrada.");
                return respuesta;
            }
            else
            {
                respuesta.Status = true;
                respuesta.Message.Add("Respuesta Eliminada.");
                var BorradoInfo = "Respuesta Id : " + id;
                respuesta.Response = BorradoInfo;
            }
            return respuesta;
        }
    }
}
