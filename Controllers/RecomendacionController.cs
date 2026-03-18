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
    public class RecomendacionController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public RecomendacionController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        //Seccion Recomendación
        [HttpPost("Nueva")]
        [EndpointSummary("Crea una recomendacion")]
        public async Task<ActionResult<RespuestaObjetoDTO>> CreateRecomendacion(CreateRecomendacionDTO model)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            //Validar foraneas
            var eventoExiste = await context.Eventos
                .AnyAsync(c => c.Id == model.EventoId);

            if (!eventoExiste)
            {
                respuesta.Message.Add("El evento al que se desea agregar no existe.");
                respuesta.Response = model.EventoId;
                return respuesta;
            }

            var categoriaExiste = await context.Cat_Recomendaciones
                .AnyAsync(ct => ct.Id == model.Categoria_RecomendacionId);
            if (!categoriaExiste)
            {
                respuesta.Message.Add("La categoria seleccionada no existe.");
                respuesta.Response = model.EventoId;
                return respuesta;
            }

            var recomendacion = mapper.Map<Recomendacion>(model);

            context.Add(recomendacion);
            await context.SaveChangesAsync();

            model.Id = recomendacion.Id;
            respuesta.Status = true;
            respuesta.Message.Add("Recomendacion creada.");
            respuesta.Response = model;

            return respuesta;
        }

        [HttpPut("Actualizar/{id:int}")]
        [EndpointSummary("Actualiza una recomendacion por Id")]
        public async Task<ActionResult<RespuestaObjetoDTO>> ActualizarRecomendacion(int id, CreateRecomendacionDTO model)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            //Buscar que exista la recomendacion
            var encontrada = await context.Recomendaciones.FirstOrDefaultAsync(x => x.Id == id);
            if (encontrada is null)
            {
                respuesta.Message.Add("Recomendacion no encontrada");
                return respuesta;
            }

            //Validar foraneas
            var eventoExiste = await context.Eventos
                .AnyAsync(c => c.Id == model.EventoId);

            if (!eventoExiste)
            {
                respuesta.Message.Add("El evento al que se desea agregar no existe.");
                respuesta.Response = model.EventoId;
                return respuesta;
            }

            var categoriaExiste = await context.Cat_Recomendaciones
                .AnyAsync(ct => ct.Id == model.Categoria_RecomendacionId);
            if (!categoriaExiste)
            {
                respuesta.Message.Add("La categoria seleccionada no existe.");
                respuesta.Response = model.EventoId;
                return respuesta;
            }

            var recomendacion = await context.Recomendaciones
                .FirstOrDefaultAsync(c => c.Id == id);

            if (recomendacion == null)
            {
                respuesta.Message.Add("Elemento no encontrado.");
                return respuesta;
            }

            model.Id = id;
            mapper.Map(model, recomendacion);

            await context.SaveChangesAsync();

            var editado = mapper.Map<CreateRecomendacionDTO>(recomendacion);

            respuesta.Status = true;
            respuesta.Message.Add("Recomendación actualizada.");
            respuesta.Response = editado;

            return respuesta;
        }

        [HttpGet("Detalles/{id:int}", Name = "Detalles de una recomendación.")]
        [EndpointSummary("Obtiene los detalles de una recomendación. Solicita por Id.")]
        public async Task<ActionResult<RespuestaObjetoDTO>> GetRecomendacionxId(int id)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var recomendacion = await context.Recomendaciones
                .Include(x => x.Evento)
                .Include(r => r.CategoriaRecomendacion)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (recomendacion is null)
            {
                respuesta.Message.Add("Recomendacion no encontrada");
                return respuesta;
            }
            else
            {
                var recomendacionDTO = mapper.Map<RecomendacionDTO>(recomendacion);
                respuesta.Status = true;
                respuesta.Message.Add("Ok");
                respuesta.Response = recomendacionDTO;
            }

            return respuesta;
        }
        [HttpDelete("Eliminar/{id:int}")]
        [EndpointSummary("Elimina una recomendacion por Id")]
        public async Task<ActionResult<RespuestaObjetoDTO>> DeleteRecomendacion(int id)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var registroABorrar = await context.Recomendaciones.Where(x => x.Id == id).ExecuteDeleteAsync();

            if (registroABorrar == 0)
            {
                respuesta.Message.Add("Recomendación No Encontrada.");
                return respuesta;
            }
            else
            {
                respuesta.Status = true;
                respuesta.Message.Add("Recomendacion Eliminada.");
                var BorradoInfo = "Recomendacion Id : " + id;
                respuesta.Response = BorradoInfo;
            }
            return respuesta;
        }

        [HttpGet("Listado")]
        [EndpointSummary("Obtiene la lista de todas las recomendaciones")]
        public async Task<ActionResult<RespuestaObjetoDTO>> GetRecomendaciones()
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var recomendaciones = await context.Recomendaciones
                .Include(x => x.Evento)
                .Include(r => r.CategoriaRecomendacion)
                .ToListAsync();
            var recomendacionesDTO = mapper.Map<IEnumerable<RecomendacionDTO>>(recomendaciones);
            respuesta.Status = true;
            respuesta.Message.Add(recomendacionesDTO.Count() + " recomendaciones.");
            respuesta.Response = recomendacionesDTO;

            return respuesta;
        }

        [HttpGet("Listado/{eventoId:int}")]
        [EndpointSummary("Obtiene la lista de todas las recomendaciones por evento. Admite categoria como filtro, opcional.")]
        public async Task<ActionResult<RespuestaObjetoDTO>> RecomendacionesPorEvento(int eventoId)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            if (eventoId <= 0)
            {
                respuesta.Message.Add("Debe proporcionar un ID de evento válido.");
                return BadRequest(respuesta);
            }

            var query = context.Recomendaciones
                .Include(x => x.Evento)
                .Include(r => r.CategoriaRecomendacion)
                .Where(r => r.EventoId == eventoId);

            //if (categoriaId.HasValue && categoriaId.Value > 0)
            //{
            //    query = query.Where(r => r.Categoria_RecomendacionId == categoriaId.Value);
            //}

            var recomendaciones = await query.ToListAsync();
            var recomendacionesDTO = mapper.Map<IEnumerable<RecomendacionDTO>>(recomendaciones);

            respuesta.Status = true;
            respuesta.Message.Add($"{recomendacionesDTO.Count()} recomendaciones encontradas.");
            respuesta.Response = recomendacionesDTO;

            return respuesta;
        }

        //Seccion Catalogo de Categorias Recomendación
        [HttpPost("CategoriaRecomendacion/Nueva")]
        [EndpointSummary("Crea una categoria de recomendacion")]
        public async Task<ActionResult<RespuestaObjetoDTO>> CreateCatRecomendacion(Categoria_RecomendacionDTO model)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var Catrecomendacion = mapper.Map<Categoria_Recomendacion>(model);

            context.Add(Catrecomendacion);
            await context.SaveChangesAsync();


            respuesta.Status = true;
            respuesta.Message.Add("Categoria de Recomendacion creada.");
            respuesta.Response = Catrecomendacion;

            return respuesta;
        }

        [HttpPut("CategoriaRecomendacion/Actualizar/{id:int}")]
        [EndpointSummary("Actualiza una recomendacion por Id")]
        public async Task<ActionResult<RespuestaObjetoDTO>> ActualizarCatRecomendacion(int id, Categoria_RecomendacionDTO model)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            // Buscar si existe la categoría en la base
            var existente = await context.Cat_Recomendaciones.FindAsync(id);

            if (existente == null)
            {
                respuesta.Message.Add("Categoría de recomendación no encontrada.");
                return respuesta;
            }

            // Mapear sobre la entidad existente
            mapper.Map(model, existente);

            context.Update(existente);
            await context.SaveChangesAsync();

            var editado = mapper.Map<Categoria_RecomendacionDTO>(existente);

            respuesta.Status = true;
            respuesta.Message.Add("Categoría de recomendación actualizada.");
            respuesta.Response = editado;


            return respuesta;
        }

        [HttpGet("CategoriaRecomendacion/Detalles/{id:int}", Name = "Detalles de una categoria.")]
        [EndpointSummary("Obtiene los detalles de una categoria de recomendación. Solicita por Id.")]
        public async Task<ActionResult<RespuestaObjetoDTO>> GetCatRecomendacionxId(int id)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var Catrecomendacion = await context.Cat_Recomendaciones.FirstOrDefaultAsync(x => x.Id == id);

            if (Catrecomendacion is null)
            {
                respuesta.Message.Add("Categoría de Recomendacion no encontrada");
                return respuesta;
            }
            else
            {
                var CatrecomendacionDTO = mapper.Map<DetalleCatRecomendacionDTO>(Catrecomendacion);
                respuesta.Status = true;
                respuesta.Message.Add("Ok");
                respuesta.Response = CatrecomendacionDTO;
            }

            return respuesta;
        }

        [HttpDelete("CategoriaRecomendacion/Eliminar/{id:int}")]
        [EndpointSummary("Elimina una categoria de recomendacion por Id")]
        public async Task<ActionResult<RespuestaObjetoDTO>> DeleteCAtRecomendacion(int id)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };
            try
            {

                var registroABorrar = await context.Cat_Recomendaciones.Where(x => x.Id == id).ExecuteDeleteAsync();

                if (registroABorrar == 0)
                {
                    respuesta.Message.Add("Categoria de Recomendación No Encontrada.");
                    return respuesta;
                }
                else
                {
                    respuesta.Status = true;
                    respuesta.Message.Add("Categoria de Recomendacion Eliminada.");
                    var BorradoInfo = "Categoria de Recomendacion Id : " + id;
                    respuesta.Response = BorradoInfo;

                    return respuesta;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("FK"))
                {
                    respuesta.Message.Add("La categoria de recomendacion tiene datos vinculados. Eliminación no permitida.");
                }
                else
                {
                    respuesta.Message.Add(ex.Message);
                }
                return respuesta;
            }
        }

        [HttpGet("CategoriaRecomendacion/Listado")]
        [EndpointSummary("Obtiene la lista de todas las categorias de recomendaciones")]
        public async Task<ActionResult<RespuestaObjetoDTO>> GetCatRecomendaciones()
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var Catrecomendaciones = await context.Cat_Recomendaciones.ToListAsync();
            var CatrecomendacionesDTO = mapper.Map<IEnumerable<DetalleCatRecomendacionDTO>>(Catrecomendaciones);
            respuesta.Status = true;
            respuesta.Message.Add(CatrecomendacionesDTO.Count() + " categorias de recomendaciones.");
            respuesta.Response = CatrecomendacionesDTO;

            return respuesta;
        }
    }
}
