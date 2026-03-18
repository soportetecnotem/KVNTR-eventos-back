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
    public class ConvencionistasController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public ConvencionistasController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpPost("Nuevo")]
        [EndpointSummary("Crea un nuevo usuario de nivel convencionista")]
        public async Task<ActionResult<RespuestaObjetoDTO>> CreateConvencionista(CreateConvDTO model)
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

            if (model.PerfilId <= 0)
            {
                respuesta.Message.Add("Debe proporcionar un IdPerfil válido.");
                respuesta.Response = model.PerfilId;
                return respuesta;
            }

            if (model.CategoriaId <= 0)
            {
                respuesta.Message.Add("Debe proporcionar una Categoría válida.");
                respuesta.Response = model.PerfilId;
                return respuesta;
            }

            // Validar existencia de claves foráneas en la base
            var perfilExiste = await context.PerfilConvencionistas
                .AnyAsync(p => p.Id == model.PerfilId);

            if (!perfilExiste)
            {
                respuesta.Message.Add("El perfil seleccionado no existe.");
                respuesta.Response = model.PerfilId;
                return respuesta;
            }

            var categoriaExiste = await context.Cat_Usuarios
                .AnyAsync(c => c.Id == model.CategoriaId);

            if (!categoriaExiste)
            {
                respuesta.Message.Add("La categoría seleccionada no existe.");
                respuesta.Response = model.CategoriaId;
                return respuesta;
            }

            var eventoExiste = await context.Eventos
                .AnyAsync(c => c.Id == model.EventoId);

            if (!eventoExiste)
            {
                respuesta.Message.Add("El evento seleccionado no existe.");
                respuesta.Response = model.EventoId;
                return respuesta;
            }

            if (await context.Convencionistas.AnyAsync(c => c.Clave == model.Clave))
            {
                respuesta.Message.Add("Ya existe un convencionista con esa clave.");
                respuesta.Response = model.Clave;
                return respuesta;
            }
            var convencionista = mapper.Map<Convencionista>(model);

            // Asociar evento
            if (eventoExiste)
            {
                // Verifica si ya existe la relación antes de agregarla
                bool yaExisteRelacion = convencionista.EventosUsuarios
                    .Any(ev => ev.EventoId == model.EventoId);

                if (!yaExisteRelacion)
                {
                    var relEvConv = new EventoUsuario { Convencionista = convencionista, EventoId = model.EventoId, Activo = true };
                    convencionista.EventosUsuarios.Add(relEvConv);
                }

            }

            context.Add(convencionista);
            await context.SaveChangesAsync();

            model.Id = convencionista.Id;

            respuesta.Status = true;
            respuesta.Message.Add("Convencionista creado");
            respuesta.Response = model;

            return respuesta;
        }

        [HttpGet("Listado")]
        [EndpointSummary("Obtiene un listado de los convencionistas")]
        public async Task<ActionResult<RespuestaObjetoDTO>> GetConvencionistas()
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var usuariosConv = await context.Convencionistas
                .Include(x => x.PerfilConvencionista)
                .Include(x => x.CategoriaUsuario)
                .Include(x => x.EventosUsuarios)
                .ThenInclude(x => x.Evento)
                .ToListAsync();
            var ConvDTO = mapper.Map<IEnumerable<DetalleConvDTO>>(usuariosConv);

            respuesta.Status = true;
            respuesta.Message.Add(ConvDTO.Count() + " usuarios convencionistas");
            respuesta.Response = ConvDTO;

            return respuesta;
        }

        [HttpGet("ListadoPaginado")]
        [EndpointSummary("Obtiene lista paginada de convencionistas")]
        public async Task<ActionResult<RespuestaObjetoDTO>> GetConvencionistas(
    int NumPagina = 1, int RegXPag = 10)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            // Calcular saltos
            var skip = (NumPagina - 1) * RegXPag;

            var query = context.Convencionistas
                .Include(x => x.PerfilConvencionista)
                .Include(x => x.CategoriaUsuario)
                .Include(x => x.EventosUsuarios)
                    .ThenInclude(x => x.Evento);

            var totalRegistros = await query.CountAsync();
            var totalPaginas = (int)Math.Ceiling((double)totalRegistros / RegXPag);

            var usuariosConv = await query
                .Skip(skip)
                .Take(RegXPag)
                .ToListAsync();

            var ConvDTO = mapper.Map<IEnumerable<DetalleConvDTO>>(usuariosConv);

            respuesta.Status = true;
            respuesta.Message.Add($"{ConvDTO.Count()} de {totalRegistros} usuarios convencionistas");
            respuesta.Response = new
            {
                TotalRegistros = totalRegistros,
                PaginaActual = NumPagina,
                TamanoPagina = RegXPag,
                TotalPaginas = totalPaginas,
                Listado = ConvDTO
            };

            return respuesta;
        }

        [HttpGet("ListadoFiltrado")]
        [EndpointSummary("Obtiene lista paginada de convencionistas con búsqueda global")]
        public async Task<ActionResult<RespuestaObjetoDTO>> GetConvencionistasFiltrado(
    [FromQuery] string? nombreConvencion = null,
    [FromQuery] string? busqueda = null,
    [FromQuery] int numPagina = 1,
    [FromQuery] int regXPag = 10)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            // Validar parámetros de paginación
            if (numPagina < 1)
            {
                respuesta.Message.Add("El número de página debe ser mayor a 0.");
                return BadRequest(respuesta);
            }

            if (regXPag < 1 || regXPag > 100)
            {
                respuesta.Message.Add("El tamaño de página debe estar entre 1 y 100.");
                return BadRequest(respuesta);
            }

            try
            {
                var query = context.Convencionistas
                    .Include(x => x.PerfilConvencionista)
                    .Include(x => x.CategoriaUsuario)
                    .Include(x => x.EventosUsuarios)
                        .ThenInclude(x => x.Evento)
                    .Where(c => c.EventosUsuarios.Any(eu => eu.Activo == true))
                    .AsQueryable();

                // Filtro por nombre de convención
                if (!string.IsNullOrWhiteSpace(nombreConvencion))
                {
                    query = query.Where(c => c.EventosUsuarios
                        .Any(eu => eu.Activo == true &&
                                  eu.Evento != null &&
                                  eu.Evento.NombreConvencion.Contains(nombreConvencion)));
                }

                // Búsqueda global en múltiples campos
                if (!string.IsNullOrWhiteSpace(busqueda))
                {
                    query = query.Where(c =>
                        c.NombreCompleto.Contains(busqueda) ||
                        c.Puesto.Contains(busqueda) ||
                        (c.PerfilConvencionista != null && c.PerfilConvencionista.Nombre.Contains(busqueda)) ||
                        (c.CategoriaUsuario != null && c.CategoriaUsuario.Nombre.Contains(busqueda))
                    );
                }

                // Contar total de registros después de aplicar filtros
                var totalRegistros = await query.CountAsync();
                var totalPaginas = (int)Math.Ceiling((double)totalRegistros / regXPag);

                // Validar que la página solicitada no exceda el total
                if (numPagina > totalPaginas && totalPaginas > 0)
                {
                    numPagina = totalPaginas;
                }

                // Aplicar paginación
                var skip = (numPagina - 1) * regXPag;
                var usuariosConv = await query
                    .OrderBy(c => c.Id)
                    .Skip(skip)
                    .Take(regXPag)
                    .ToListAsync();

                var convDTO = mapper.Map<IEnumerable<DetalleConvDTO>>(usuariosConv);

                // Construir mensaje descriptivo
                var filtrosAplicados = new List<string>();
                if (!string.IsNullOrWhiteSpace(nombreConvencion))
                    filtrosAplicados.Add($"Convención: '{nombreConvencion}'");
                if (!string.IsNullOrWhiteSpace(busqueda))
                    filtrosAplicados.Add($"Búsqueda global: '{busqueda}'");

                var mensajeFiltros = filtrosAplicados.Any()
                    ? $" con filtros: {string.Join(", ", filtrosAplicados)}"
                    : "";

                respuesta.Status = true;
                respuesta.Message.Add($"{convDTO.Count()} de {totalRegistros} convencionistas{mensajeFiltros}");
                respuesta.Response = new
                {
                    TotalRegistros = totalRegistros,
                    PaginaActual = numPagina,
                    TamanoPagina = regXPag,
                    TotalPaginas = totalPaginas,
                    Listado = convDTO
                };

                return Ok(respuesta);
            }
            catch (Exception ex)
            {
                respuesta.Message.Add($"Error al obtener convencionistas filtrados: {ex.Message}");
                return StatusCode(500, respuesta);
            }
        }

        [HttpGet("ListadoXEvento/{idEvento:int}")]
        [EndpointSummary("Obtiene un listado de los convencionistas pertenecientes a un evento")]
        public async Task<ActionResult<RespuestaObjetoDTO>> ListadoConvencionistasEvento(int idEvento)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var usuariosConv = await context.Convencionistas
                    .Include(x => x.PerfilConvencionista)
                    .Include(x => x.CategoriaUsuario)
                    .Include(x => x.EventosUsuarios.Where(eu => eu.EventoId == idEvento)) // Solo ese evento
                        .ThenInclude(eu => eu.Evento)
                    .Where(c => c.EventosUsuarios.Any(eu => eu.EventoId == idEvento && eu.Activo == true))
                    .ToListAsync();

            var ConvDTO = mapper.Map<IEnumerable<DetalleConvDTO>>(usuariosConv);

            respuesta.Status = true;
            respuesta.Message.Add(ConvDTO.Count() + " convencionistas");
            respuesta.Response = ConvDTO;

            return respuesta;
        }

        [HttpGet("Detalles/{id:int}")]
        [EndpointSummary("Obtiene los detalles de un convencionista. Buscado por ID")]
        public async Task<ActionResult<RespuestaObjetoDTO>> ConvencionistaXId(int id)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var convencionista = await context.Convencionistas
                .Include(x => x.PerfilConvencionista)
                .Include(x => x.CategoriaUsuario)
                .Include(x => x.EventosUsuarios)
                .ThenInclude(x => x.Evento)
                 .Where(c => c.EventosUsuarios.Any(eu => eu.Activo == true))
                    .FirstOrDefaultAsync(x => x.Id == id);

            if (convencionista is null)
            {
                respuesta.Message.Add("Usuario convencionista no encontrado.");
                return respuesta;
            }
            else
            {
                var ConvDTO = mapper.Map<DetalleConvDTO>(convencionista);
                respuesta.Status = true;
                respuesta.Message.Add("Ok");
                respuesta.Response = ConvDTO;
            }

            return respuesta;
        }

        [HttpPut("Actualizar/{id:int}")]
        [EndpointSummary("Actualiza un usuario convencionista por Id")]
        public async Task<ActionResult<RespuestaObjetoDTO>> ActualizarConvencionista(int id, CreateConvDTO model)
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

            if (model.PerfilId <= 0)
            {
                respuesta.Message.Add("Debe proporcionar un IdPerfil válido.");
                respuesta.Response = model.PerfilId;
                return respuesta;
            }

            if (model.CategoriaId <= 0)
            {
                respuesta.Message.Add("Debe proporcionar una Categoría válida.");
                respuesta.Response = model.PerfilId;
                return respuesta;
            }

            // Validar existencia de claves foráneas en la base
            var perfilExiste = await context.PerfilConvencionistas
                .AnyAsync(p => p.Id == model.PerfilId);

            if (!perfilExiste)
            {
                respuesta.Message.Add("El perfil seleccionado no existe.");
                respuesta.Response = model.PerfilId;
                return respuesta;
            }

            var categoriaExiste = await context.Cat_Usuarios
                .AnyAsync(c => c.Id == model.CategoriaId);

            if (!categoriaExiste)
            {
                respuesta.Message.Add("La categoría seleccionada no existe.");
                respuesta.Response = model.CategoriaId;
                return respuesta;
            }

            var eventoExiste = await context.Eventos
                .AnyAsync(c => c.Id == model.EventoId);

            if (!eventoExiste)
            {
                respuesta.Message.Add("El evento seleccionado no existe.");
                respuesta.Response = model.EventoId;
                return respuesta;
            }

            if (await context.Convencionistas.AnyAsync(c => c.Clave == model.Clave && c.Id != id))
            {
                respuesta.Message.Add("Ya existe otro convencionista con esa clave.");
                respuesta.Response = model.Clave;
                return respuesta;
            }

            var convencionista = await context.Convencionistas
                .Include(c => c.EventosUsuarios)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (convencionista == null)
            {
                respuesta.Message.Add("El convencionista no fue encontrado.");
                return respuesta;
            }

            model.Id = id;
            mapper.Map(model, convencionista);


            // Desactivar todas las relaciones actuales
            foreach (var relacion in convencionista.EventosUsuarios)
            {
                relacion.Activo = false;
            }

            // Ver si ya existe la relación con el evento nuevo
            var relacionActual = convencionista.EventosUsuarios
                .FirstOrDefault(ev => ev.EventoId == model.EventoId);

            if (relacionActual != null)
            {
                relacionActual.Activo = true;
            }
            else
            {
                convencionista.EventosUsuarios.Add(new EventoUsuario
                {
                    EventoId = model.EventoId,
                    ConvencionistaId = id,
                    Activo = true
                });
            }

            await context.SaveChangesAsync();

            var editado = mapper.Map<CreateConvDTO>(convencionista);

            respuesta.Status = true;
            respuesta.Message.Add("Información actualizada.");
            respuesta.Response = editado;

            return respuesta;
        }

        [HttpDelete("{id:int}")]
        [EndpointSummary("Elimina un usuario convencionista por Id")]
        public async Task<ActionResult<RespuestaObjetoDTO>> DeleteConvecionista(int id)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            try
            {
                var registroABorrar = await context.Convencionistas.Where(x => x.Id == id).ExecuteDeleteAsync();

                if (registroABorrar == 0)
                {
                    respuesta.Message.Add("Usuario convencionista no encontrado.");
                    return respuesta;
                }
                else
                {
                    respuesta.Status = true;
                    respuesta.Message.Add("Usuario Eliminado");
                    var BorradoInfo = "Usuario Id :" + id;
                    respuesta.Response = BorradoInfo;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("FK"))
                {
                    respuesta.Message.Add("El convencionista tiene datos vinculados. Eliminación no permitida.");
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
