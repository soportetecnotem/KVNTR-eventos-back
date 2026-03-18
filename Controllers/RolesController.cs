using EventosBack.DTO.Responses;
using EventosBack.DTO.Roles;
using EventosBack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventosBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<Usuario> _userManager;

        public RolesController(RoleManager<IdentityRole> roleManager, UserManager<Usuario> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        [HttpPost("Nuevo")]
        //[Authorize]
        [EndpointSummary("Crea un Rol de Usuario")]
        public async Task<ActionResult<RespuestaGeneralDTO>> CrearRol([FromBody] CrearRolDTO dto)
        {
            var respuesta = new RespuestaGeneralDTO
            {
                Message = []
            };

            if (await _roleManager.RoleExistsAsync(dto.Nombre))
            {
                respuesta.Message.Add("El rol ya existe");
                return respuesta;
            }

            var resultado = await _roleManager.CreateAsync(new IdentityRole(dto.Nombre));

            if (resultado.Succeeded)
            {
                respuesta.Message.Add("Rol creado");
                respuesta.Status = true;
                return respuesta;
            }
            else
            {
                var errores = TraducirErroresIdentity(resultado.Errors);
                respuesta.Message = errores;
            }

            return respuesta;

        }

        [HttpPut("Actualizar/{id}")]
        [Authorize]
        [EndpointSummary("Actualiza el nombre de Rol de Usuario")]
        public async Task<ActionResult<RespuestaGeneralDTO>> EditarRol(string id, [FromBody] EditarRolDTO dto)
        {
            var respuesta = new RespuestaGeneralDTO
            {
                Message = []
            };

            var rol = await _roleManager.FindByIdAsync(id);
            if (rol == null)
            {
                respuesta.Message.Add("Rol no encontrado");

                return respuesta;
            }

            rol.Name = dto.NuevoNombre;
            var resultado = await _roleManager.UpdateAsync(rol);

            if (resultado.Succeeded)
            {
                respuesta.Message.Add("Rol actualizado");
                respuesta.Status = true;
                return respuesta;
            }
            else
            {
                var errores = TraducirErroresIdentity(resultado.Errors);
                respuesta.Message = errores;
            }

            return respuesta;

        }

        // GET: api/roles
        [HttpGet("Listado")]
        [Authorize]
        [EndpointSummary("Obtiene los Roles de Usuario")]
        public async Task<ActionResult<RespuestaObjetoDTO>> ObtenerRoles()
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };
            var roles = await _roleManager.Roles.ToListAsync();

            if (roles == null)
            {
                respuesta.Message.Add("No se obtuvieron roles.");
            }
            else
            {
                respuesta.Status = true;
                respuesta.Message.Add($"{roles.Count} roles");
                respuesta.Response = roles;
            }
            return respuesta;
        }

        // POST: api/roles/asignar
        [HttpPost("asignar-rol")]
        [Authorize]
        [EndpointSummary("Asigna un rol a un Usuario")]
        public async Task<ActionResult<RespuestaObjetoDTO>> AsignarRolComoClaim(AsignarRolDTO dto)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var usuario = await _userManager.FindByNameAsync(dto.UserName);
            if (usuario == null)
            {
                respuesta.Message.Add("Usuario no encontrado");
            }
            else
            {
                var existe = await _roleManager.RoleExistsAsync(dto.Rol);
                if (!existe)
                {
                    respuesta.Message.Add("Rol no existe");
                }
                else
                {
                    var rolesUsuario = await _userManager.GetRolesAsync(usuario);
                    if (rolesUsuario.Contains(dto.Rol))
                    {
                        respuesta.Message.Add("El usuario ya tiene ese rol asignado");
                    }
                    else
                    {
                        var resultado = await _userManager.AddToRoleAsync(usuario, dto.Rol);
                        if (!resultado.Succeeded)
                        {
                            respuesta.Message.Add("Error al asignar el rol");
                        }
                        else
                        {
                            respuesta.Status = true;
                            respuesta.Message.Add("Rol asignado al usuario");
                        }

                    }

                }

            }
            return respuesta;
        }

        [HttpGet("Detalles/{id}")]
        [Authorize]
        [EndpointSummary("Obtiene los detalles de un rol por su nombre.")]
        public async Task<ActionResult<RespuestaObjetoDTO>> GetRolPorNombre(string id)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var rol = await _roleManager.FindByIdAsync(id);

            if (rol == null)
            {
                respuesta.Message.Add("El rol no fue encontrado.");
                return respuesta;
            }

            var rolDTO = new
            {
                rol.Id,
                rol.Name,
                rol.NormalizedName
            };

            respuesta.Status = true;
            respuesta.Message.Add("Rol encontrado.");
            respuesta.Response = rolDTO;
            return Ok(respuesta);
        }

        //Eliminar rol a usuario
        [HttpPost("remover-rol")]
        [Authorize]
        [EndpointSummary("Elimina un rol asignado a un Usuario")]
        public async Task<ActionResult<RespuestaObjetoDTO>> EliminarRolAUsuario(AsignarRolDTO dto)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var usuario = await _userManager.FindByNameAsync(dto.UserName);
            if (usuario == null)
            {
                respuesta.Message.Add("Usuario no encontrado");
            }
            else
            {
                var existe = await _roleManager.RoleExistsAsync(dto.Rol);
                if (!existe)
                {
                    respuesta.Message.Add("Rol no existe");
                }
                else
                {
                    var rolesUsuario = await _userManager.GetRolesAsync(usuario);
                    if (!rolesUsuario.Contains(dto.Rol))
                    {
                        respuesta.Message.Add("El usuario no tiene ese rol asignado");
                    }
                    else
                    {
                        var resultado = await _userManager.RemoveFromRoleAsync(usuario, dto.Rol);
                        if (!resultado.Succeeded)
                        {
                            respuesta.Message.Add("No se pudo eliminar el rol");
                        }
                        else
                        {
                            respuesta.Status = true;
                            respuesta.Message.Add("Rol eliminado del usuario");
                        }

                    }

                }

            }
            return respuesta;
        }

        [HttpDelete("{id}")]
        [Authorize]
        [EndpointSummary("Elimina un rol del sistema por Id")]
        public async Task<ActionResult<RespuestaObjetoDTO>> EliminarRol(string id)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var rol = await _roleManager.FindByIdAsync(id);
            try
            {
                if (rol == null)
                {
                    respuesta.Message.Add("Rol no encontrado");
                    return respuesta;
                }
                else
                {
                    var resultado = await _roleManager.DeleteAsync(rol);
                    if (!resultado.Succeeded)
                    {
                        respuesta.Message.Add("No se pudo eliminar el rol");
                        return respuesta;
                    }
                    else
                    {
                        respuesta.Status = true;
                        respuesta.Message.Add("Rol Eliminado");
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException!.Message.Contains("FK"))
                {
                    respuesta.Message.Add("El rol de usuario tiene datos vinculados. Eliminación no permitida.");
                }
                else
                {
                    respuesta.Message.Add(ex.Message);
                }
                return respuesta;
            }
            return respuesta;
        }

        [HttpGet("api/test-roles")]
        [EndpointSummary("obtiene un listado de roles de Usuario para realizar el test al endpoint")]
        public IActionResult TestRoles()
        {
            var roles = _roleManager.Roles.ToList();
            return Ok(roles);
        }

        private List<string> TraducirErroresIdentity(IEnumerable<IdentityError> errores)
        {
            var lista = new List<string>();

            foreach (var error in errores)
            {
                string mensajeTraducido = error.Code switch
                {
                    "DuplicateRoleName" => "Ese nombre de rol ya existe.",
                    "InvalidRoleName" => "El nombre del rol no es válido.",
                    "TooShort" => "El nombre del rol es demasiado corto.",
                    _ => error.Description // fallback por si no lo conoces
                };

                lista.Add(mensajeTraducido);
            }

            return lista;
        }

    }
}