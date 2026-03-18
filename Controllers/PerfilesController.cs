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
    [Route("api/PerfilConvencionista")]
    [ApiController]
    [Authorize]
    public class PerfilesController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public PerfilesController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpPost("Nuevo")]
        [EndpointSummary("Crea un perfil para convencionista")]
        public async Task<ActionResult<RespuestaObjetoDTO>> CreatePerfil(PerfilDTO model)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var perfil = mapper.Map<PerfilConvencionista>(model);
            context.Add(perfil);
            await context.SaveChangesAsync();

            model.Id = perfil.Id;

            respuesta.Status = true;
            respuesta.Message.Add("Perfil creado");
            respuesta.Response = model;

            return respuesta;
        }

        [HttpGet("Listado")]
        [EndpointSummary("Obtiene la lista de todos los perfiles")]
        public async Task<ActionResult<RespuestaObjetoDTO>> GetPerfiles()
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var perfiles = await context.PerfilConvencionistas.ToListAsync();
            var perfilesDTO = mapper.Map<IEnumerable<PerfilDTO>>(perfiles);
            respuesta.Status = true;
            respuesta.Message.Add(perfilesDTO.Count() + " perfiles");
            respuesta.Response = perfilesDTO;

            return respuesta;
        }

        [HttpPut("Actualizar/{id:int}")]
        [EndpointSummary("Actualiza un Perfil por Id")]
        public async Task<ActionResult<RespuestaObjetoDTO>> ActualizarPerfil(int id, PerfilDTO model)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var perfil = mapper.Map<PerfilConvencionista>(model);
            perfil.Id = id;
            context.Update(perfil);
            await context.SaveChangesAsync();

            var editado = mapper.Map<PerfilDTO>(perfil);

            respuesta.Status = true;
            respuesta.Message.Add("Perfil actualizado");
            respuesta.Response = editado;

            return respuesta;
        }

        [HttpGet("Detalles/{id:int}")]
        [EndpointSummary("Obtiene los detalles de un perfil. Solicita por Id.")]
        public async Task<ActionResult<RespuestaObjetoDTO>> GetPerfilId(int id)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var perfil = await context.PerfilConvencionistas
                .FirstOrDefaultAsync(x => x.Id == id);

            if (perfil is null)
            {
                respuesta.Message.Add("Perfil no encontrado");
                return respuesta;
            }
            else
            {
                var perfilDTO = mapper.Map<PerfilDTO>(perfil);
                respuesta.Status = true;
                respuesta.Message.Add("Ok");
                respuesta.Response = perfilDTO;
            }

            return respuesta;
        }

        [HttpDelete("{id:int}")]
        [EndpointSummary("Elimina un perfil de convencionista por Id")]
        public async Task<ActionResult<RespuestaObjetoDTO>> DeletePerfil(int id)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            try
            {
                var registroABorrar = await context.PerfilConvencionistas.Where(x => x.Id == id).ExecuteDeleteAsync();

                if (registroABorrar == 0)
                {
                    respuesta.Message.Add("Perfil No Encontrado.");
                    return respuesta;
                }
                else
                {
                    respuesta.Status = true;
                    respuesta.Message.Add("Perfil Eliminado");
                    var BorradoInfo = "Perfil Id : " + id;
                    respuesta.Response = BorradoInfo;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("FK"))
                {
                    respuesta.Message.Add("El perfil de convencionista tiene datos vinculados. Eliminación no permitida.");
                }
                else
                {
                    respuesta.Message.Add(ex.Message);
                }
                return respuesta;
            }
            return respuesta;
        }
    }
}
