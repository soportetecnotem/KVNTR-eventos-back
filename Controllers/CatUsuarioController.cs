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
    [Route("api/CategoriaConvencionista")]
    [ApiController]
    [Authorize]
    public class CatUsuarioController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public CatUsuarioController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpPost("Nueva")]
        [EndpointSummary("Crea una categoria de convencionista")]
        public async Task<ActionResult<RespuestaObjetoDTO>> CreateCategoria(CategoriaUDTO model)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var categoria = mapper.Map<CategoriaUsuario>(model);
            context.Add(categoria);
            await context.SaveChangesAsync();

            model.Id = categoria.Id;

            respuesta.Status = true;
            respuesta.Message.Add("Categoria creada.");
            respuesta.Response = model;

            return respuesta;
        }

        [HttpGet("Listado")]
        [EndpointSummary("Obtiene la lista de todas las categorias")]
        public async Task<ActionResult<RespuestaObjetoDTO>> GetCategorias()
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var categorias = await context.Cat_Usuarios
                //.Where(e => e.Activo)
                .ToListAsync();
            var categoriasDTO = mapper.Map<IEnumerable<CategoriaUDTO>>(categorias);
            respuesta.Status = true;
            respuesta.Message.Add(categoriasDTO.Count() + " categorías.");
            respuesta.Response = categoriasDTO;

            return respuesta;
        }

        [HttpPut("Actualizar/{id:int}")]
        [EndpointSummary("Actualiza una categoria por Id")]
        public async Task<ActionResult<RespuestaObjetoDTO>> ActualizarCategoria(int id, CategoriaUDTO model)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var categoria = mapper.Map<CategoriaUsuario>(model);
            categoria.Id = id;
            context.Update(categoria);
            await context.SaveChangesAsync();

            var editado = mapper.Map<CategoriaUDTO>(categoria);

            respuesta.Status = true;
            respuesta.Message.Add("Categoria actualizada.");
            respuesta.Response = editado;

            return respuesta;
        }

        [HttpGet("Detalles/{id:int}")]
        [EndpointSummary("Obtiene los detalles de una categoria. Buscado por ID")]
        public async Task<ActionResult<RespuestaObjetoDTO>> CategoriaXId(int id)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var categoria = await context.Cat_Usuarios
                .FirstOrDefaultAsync(x => x.Id == id);

            if (categoria is null)
            {
                respuesta.Message.Add("Categoria de convencionista no encontrada.");
                return respuesta;
            }
            else
            {
                var CatDTO = mapper.Map<CategoriaUsuario>(categoria);
                respuesta.Status = true;
                respuesta.Message.Add("Ok");
                respuesta.Response = CatDTO;
            }

            return respuesta;
        }

        [HttpDelete("{id:int}")]
        [EndpointSummary("Elimina una categoria de convencionista por Id")]
        public async Task<ActionResult<RespuestaObjetoDTO>> DeleteCategoriaUsu(int id)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            try
            {
                var registroABorrar = await context.Cat_Usuarios.Where(x => x.Id == id).ExecuteDeleteAsync();

                if (registroABorrar == 0)
                {
                    respuesta.Message.Add("Categoria convencionista no encontrada.");
                    return respuesta;
                }
                else
                {
                    respuesta.Status = true;
                    respuesta.Message.Add("Categoria de convencionista eliminada");
                    var BorradoInfo = "Categoria Id :" + id;
                    respuesta.Response = BorradoInfo;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("FK"))
                {
                    respuesta.Message.Add("La categoria de convencionista tiene datos vinculados. Eliminación no permitida.");
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
