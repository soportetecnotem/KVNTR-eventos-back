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
    public class ModulosController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public ModulosController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpPost("Nuevo")]
        [EndpointSummary("Crea un modulo.")]
        public async Task<ActionResult<RespuestaObjetoDTO>> CreateModulo(ModuloDTO model)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var modulo = mapper.Map<Modulo>(model);
            modulo.FMod = DateTime.Now;

            context.Add(modulo);
            await context.SaveChangesAsync();

            respuesta.Status = true;
            respuesta.Message.Add("Modulo creado.");
            respuesta.Response = modulo;

            return respuesta;
        }

        [HttpPut("Actualizar/{Id}")]
        [EndpointSummary("Actualiza un modulo por Id")]
        public async Task<ActionResult<RespuestaObjetoDTO>> ActualizarModulo(int Id, ModuloDTO model)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var encontrado = await context.Modulos.FirstOrDefaultAsync(x => x.Id == Id);
            if (encontrado is null)
            {
                respuesta.Message.Add("Modulo no encontrado.");
                return respuesta;
            }

            mapper.Map(model, encontrado);
            encontrado.FMod = DateTime.Now;

            await context.SaveChangesAsync();

            var editado = mapper.Map<DetalleModuloDTO>(encontrado);

            respuesta.Status = true;
            respuesta.Message.Add("Modulo actualizado.");
            respuesta.Response = editado;

            return respuesta;
        }

        [HttpGet("Detalles/{KeyCode}")]
        [EndpointSummary("Obtiene los detalles de un modulo. Solicita por Id.")]
        public async Task<ActionResult<RespuestaObjetoDTO>> GetmoduloxId(string KeyCode)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var modulo = await context.Modulos
                .FirstOrDefaultAsync(x => x.KeyCode == KeyCode);

            if (modulo is null)
            {
                respuesta.Message.Add("Modulo no encontrado.");
                return respuesta;
            }
            else
            {
                var moduloDTO = mapper.Map<DetalleModuloDTO>(modulo);
                respuesta.Status = true;
                respuesta.Message.Add("Ok");
                respuesta.Response = moduloDTO;
            }

            return respuesta;
        }

        [HttpGet("Listado")]
        [EndpointSummary("Obtiene la lista de todos los modulos.")]
        public async Task<ActionResult<RespuestaObjetoDTO>> ListadoModulos()
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var modulo = await context.Modulos
                .ToListAsync();

            var moduloDTO = mapper.Map<IEnumerable<DetalleModuloDTO>>(modulo);
            respuesta.Status = true;
            respuesta.Message.Add(moduloDTO.Count() + " modulos.");
            respuesta.Response = moduloDTO;

            return respuesta;
        }

        [HttpDelete("Eliminar/{Id}")]
        [EndpointSummary("Elimina un modulo por Id")]
        public async Task<ActionResult<RespuestaObjetoDTO>> DeleteModulo(int Id)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var registroABorrar = await context.Modulos.Where(x => x.Id == Id).ExecuteDeleteAsync();

            if (registroABorrar == 0)
            {
                respuesta.Message.Add("Modulo no encontrado.");
                return respuesta;
            }
            else
            {
                respuesta.Status = true;
                respuesta.Message.Add("Modulo Eliminado.");
                var BorradoInfo = "Modulo Id : " + Id;
                respuesta.Response = BorradoInfo;
            }
            return respuesta;
        }

    }
}
