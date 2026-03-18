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
    public class ActividadesController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public ActividadesController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpPost("Nueva")]
        [EndpointSummary("Crea una nueva actividad")]
        public async Task<ActionResult<RespuestaObjetoDTO>> NuevaActividad(ActividadDTO model)
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

            if (model.Categoria_ActividadesId <= 0)
            {
                respuesta.Message.Add("Debe proporcionar una categoría válida.");
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

            var categoriaActvExiste = await context.Cat_Actividades
                .AnyAsync(ct => ct.Id == model.Categoria_ActividadesId);

            if (!categoriaActvExiste)
            {
                respuesta.Message.Add("La categoria de activiad seleccionada no existe.");
                respuesta.Response = model.Categoria_ActividadesId;
                return respuesta;
            }

            if (model.Titulo == "")
            {
                respuesta.Message.Add("El titulo de la actividad no puede estar vacio.");
                return respuesta;
            }
            else
            {
                var actividad = mapper.Map<Actividad>(model);

                // Asociar usuarios si vienen en el modelo
                if (model.ConvencionistasIds?.Any() == true)
                {
                    actividad.Convencionistas = model.ConvencionistasIds.Select(cId => new ActividadUsuario
                    {
                        ConvencionistaId = cId
                    }).ToList();
                }

                context.Add(actividad);
                await context.SaveChangesAsync();

                respuesta.Status = true;
                respuesta.Message.Add("Actividad agregada");
                model.Id = actividad.Id;
                respuesta.Response = model;
            }
            return respuesta;
        }

        [HttpPut("Actualizar/{id:int}")]
        [EndpointSummary("Actualiza una actividad por Id")]
        public async Task<ActionResult<RespuestaObjetoDTO>> ActualizarActividad(int id, ActualizarActividadDTO model)
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

            if (model.Categoria_ActividadesId <= 0)
            {
                respuesta.Message.Add("Debe proporcionar una categoría válida.");
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

            var categoriaActvExiste = await context.Cat_Actividades
                .AnyAsync(ct => ct.Id == model.Categoria_ActividadesId);

            if (!categoriaActvExiste)
            {
                respuesta.Message.Add("La categoria de activiad seleccionada no existe.");
                respuesta.Response = model.Categoria_ActividadesId;
                return respuesta;
            }

            var actividad = await context.Actividades
                .Include(a => a.Convencionistas)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (actividad == null)
            {
                respuesta.Message.Add("La actividad no fue encontrada.");
                respuesta.Response = id;
                return respuesta;
            }

            mapper.Map(model, actividad);

            // Asociar usuarios si vienen en el modelo
            // Verificar que todos los IDs recibidos existen
            var idsValidos = await context.Convencionistas
                .Where(c => model.ConvencionistasIds.Contains(c.Id))
                .Select(c => c.Id)
                .ToListAsync();

            var idsRecibidos = model.ConvencionistasIds ?? [];

            // Eliminar relaciones que ya no están en el modelo
            actividad.Convencionistas.RemoveAll(r => !idsRecibidos.Contains(r.ConvencionistaId));

            // Agregar nuevas relaciones si no existen
            var idsRelacionados = actividad.Convencionistas.Select(r => r.ConvencionistaId).ToList();

            var nuevosIds = idsValidos.Except(idsRelacionados);

            foreach (var nuevoId in nuevosIds)
            {
                actividad.Convencionistas.Add(new ActividadUsuario
                {
                    ConvencionistaId = nuevoId
                });
            }

            await context.SaveChangesAsync();

            var editado = mapper.Map<ActividadDTO>(actividad);
            editado.ConvencionistasIds = idsRecibidos;

            respuesta.Status = true;
            respuesta.Message.Add("Actividad actualizada.");
            respuesta.Response = editado;

            return respuesta;
        }

        [HttpGet("Detalles/{id:int}")]
        [EndpointSummary("Obtiene los detalles de una actividad. Solicita por Id.")]
        public async Task<ActionResult<RespuestaObjetoDTO>> DetallesActividad(int id)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var actividad = await context.Actividades
                .Include(x => x.Evento)
                .Include(r => r.Categoria)
                .Include(a => a.Convencionistas)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (actividad is null)
            {
                respuesta.Message.Add("Actividad no encontrada");
                return respuesta;
            }
            else
            {
                var actividadDTO = mapper.Map<DetallesActividadDTO>(actividad);
                respuesta.Status = true;
                respuesta.Message.Add("Ok");
                respuesta.Response = actividadDTO;
            }

            return respuesta;
        }

        [HttpDelete("Eliminar/{id:int}")]
        [EndpointSummary("Elimina una actividad por Id")]
        public async Task<ActionResult<RespuestaObjetoDTO>> DeleteActividad(int id)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var actividad = await context.Actividades
                .Include(a => a.Convencionistas)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (actividad == null)
            {
                respuesta.Message.Add("Actividad no encontrada.");
                respuesta.Response = $"Actividad Id: {id}";
                return respuesta;
            }

            // Eliminar relaciones hijas primero (ActividadUsuario)
            context.RemoveRange(actividad.Convencionistas);

            // Luego eliminar la actividad
            context.Actividades.Remove(actividad);
            await context.SaveChangesAsync();

            respuesta.Status = true;
            respuesta.Message.Add("Actividad eliminada.");
            respuesta.Response = $"Actividad Id: {id}";

            return respuesta;
        }

        [HttpGet("Listado")]
        [EndpointSummary("Obtiene la lista de todas las actividades")]
        public async Task<ActionResult<RespuestaObjetoDTO>> GetActividades()
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var actividades = await context.Actividades
                .Include(x => x.Evento)
                .Include(x => x.Categoria)
                .ToListAsync();
            var actividadesDTO = mapper.Map<IEnumerable<ListaActividadDTO>>(actividades);
            respuesta.Status = true;
            respuesta.Message.Add(actividadesDTO.Count() + " actividades.");
            respuesta.Response = actividadesDTO;

            return respuesta;
        }

        [HttpGet("Listado/{convencionistaId:int}/{eventoId:int}")]
        [EndpointSummary("Obtiene las actividades de un convencionista en un evento específico")]
        public async Task<ActionResult<RespuestaObjetoDTO>> GetActividadesXConvencionista(int convencionistaId, int eventoId)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var actividades = await context.Actividades
                .Include(a => a.Evento)
                .Include(a => a.Categoria)
                .Include(a => a.Convencionistas)
                    .ThenInclude(au => au.Convencionista)
                .Where(a => a.EventoId == eventoId &&
                            a.Convencionistas.Any(au => au.ConvencionistaId == convencionistaId))
                .OrderBy(a => a.Fecha) // actividades más próximas primero
                .ToListAsync();

            if (actividades.Any())
            {
                var actividadesDTO = mapper.Map<IEnumerable<ListaActividadDTO>>(actividades);
                respuesta.Status = true;
                respuesta.Message.Add($"{actividadesDTO.Count()} actividades encontradas para el convencionista en el evento {eventoId}.");
                respuesta.Response = actividadesDTO;
            }
            else
            {
                respuesta.Status = false;
                respuesta.Message.Add("No se encontraron actividades para este convencionista en el evento especificado.");
            }

            return respuesta;
        }

        //Seccion Catalogo de Categorias Actividades
        [HttpPost("CategoriaActividades/Nueva")]
        [EndpointSummary("Crea una categoria de actividades")]
        public async Task<ActionResult<RespuestaObjetoDTO>> CreateCatActividad(Categoria_ActividadDTO model)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var CatActividad = mapper.Map<Categoria_Actividades>(model);

            context.Add(CatActividad);
            await context.SaveChangesAsync();

            model.Id = CatActividad.Id;
            respuesta.Status = true;
            respuesta.Message.Add("Categoria de Actividad creada.");
            respuesta.Response = model;

            return respuesta;
        }

        [HttpPut("CategoriaActividades/Actualizar/{id:int}")]
        [EndpointSummary("Actualiza una categoria de actividades por Id")]
        public async Task<ActionResult<RespuestaObjetoDTO>> ActualizarCatActividades(int id, Categoria_ActividadDTO model)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var CatActividad = mapper.Map<Categoria_Actividades>(model);
            CatActividad.Id = id;
            context.Update(CatActividad);
            await context.SaveChangesAsync();

            var editado = mapper.Map<Categoria_ActividadDTO>(CatActividad);
            editado.Id = id;

            respuesta.Status = true;
            respuesta.Message.Add("Categoria de actividad actualizada.");
            respuesta.Response = editado;

            return respuesta;
        }

        [HttpGet("CategoriaActividades/Detalles/{id:int}")]
        [EndpointSummary("Obtiene los detalles de una categoria de actividades. Solicita por Id.")]
        public async Task<ActionResult<RespuestaObjetoDTO>> GetCatActividad(int id)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var CatActividad = await context.Cat_Actividades.FirstOrDefaultAsync(x => x.Id == id);

            if (CatActividad is null)
            {
                respuesta.Message.Add("Categoría de Actividad no encontrada");
                return respuesta;
            }
            else
            {
                var CatActividadDTO = mapper.Map<Categoria_ActividadDTO>(CatActividad);
                respuesta.Status = true;
                respuesta.Message.Add("Ok");
                respuesta.Response = CatActividadDTO;
            }

            return respuesta;
        }

        [HttpDelete("CategoriaActividades/Eliminar/{id:int}")]
        [EndpointSummary("Elimina una categoria de actividades por Id")]
        public async Task<ActionResult<RespuestaObjetoDTO>> DeleteCatActividad(int id)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };
            try
            {
                var registroABorrar = await context.Cat_Actividades.Where(x => x.Id == id).ExecuteDeleteAsync();

                if (registroABorrar == 0)
                {
                    respuesta.Message.Add("Categoria de actividades no encontrada.");
                    return respuesta;
                }
                else
                {
                    respuesta.Status = true;
                    respuesta.Message.Add("Categoria de actividades eliminada.");
                    var BorradoInfo = "Categoria de actividades Id : " + id;
                    respuesta.Response = BorradoInfo;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("FK"))
                {
                    respuesta.Message.Add("La categoria tiene datos vinculados. Eliminación no permitida.");
                }
                else
                {
                    respuesta.Message.Add(ex.Message);
                }
                return respuesta;
            }
            return respuesta;
        }

        [HttpGet("CategoriaActividades/Listado")]
        [EndpointSummary("Obtiene la lista de todas las categorias de actividades")]
        public async Task<ActionResult<RespuestaObjetoDTO>> GetCatActividades()
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var CatActividades = await context.Cat_Actividades.ToListAsync();
            var CatActividadesDTO = mapper.Map<IEnumerable<Categoria_ActividadDTO>>(CatActividades);
            respuesta.Status = true;
            respuesta.Message.Add(CatActividadesDTO.Count() + " categorias de actividades.");
            respuesta.Response = CatActividadesDTO;

            return respuesta;
        }

    }
}
