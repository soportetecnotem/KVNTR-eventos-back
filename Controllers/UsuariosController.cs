using AutoMapper;
using EventosBack.Data;
using EventosBack.DTO;
using EventosBack.DTO.Responses;
using EventosBack.Models;
using EventosBack.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EventosBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<Usuario> userManager;
        private readonly IConfiguration configuration;
        private readonly SignInManager<Usuario> signInManager;
        private readonly IServiciosUsuarios serviciosUsuarios;
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public UsuariosController(RoleManager<IdentityRole> roleManager, UserManager<Usuario> userManager,
            IConfiguration configuration, SignInManager<Usuario> signInManager,
            IServiciosUsuarios serviciosUsuarios, ApplicationDbContext context,
            IMapper mapper)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.configuration = configuration;
            this.signInManager = signInManager;
            this.serviciosUsuarios = serviciosUsuarios;
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet("Listado")]
        [Authorize]
        [EndpointSummary("Obtiene la lista de todos los usuarios.")]
        public async Task<ActionResult<RespuestaGeneralDTO>> Get()
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };
            try
            {
                var usuarios = await context.Users
                .OrderBy(x => x.FechaCreacion)
                .ToListAsync();

                var usuariosDTO = mapper.Map<List<UsuarioDTO>>(usuarios);

                foreach (var usuarioDTO in usuariosDTO)
                {
                    var usuario = await userManager.FindByNameAsync(usuarioDTO.UserName!);
                    if (usuario != null)
                    {
                        usuarioDTO.Roles = (await userManager.GetRolesAsync(usuario)).ToList();
                    }
                }
                respuesta.Response = usuariosDTO;
                return respuesta;
            }
            catch (Exception ex)
            {
                respuesta.Message.Add(ex.Message);
                return respuesta;
            }
        }

        [HttpPost("Nuevo")]
        [Authorize]
        [EndpointSummary("Registrar nuevo usuario.")]
        public async Task<ActionResult<RespuestaGeneralDTO>> Registrar(CreacionUsuarioDTO UsuarioDTO)
        {
            var respuesta = new RespuestaGeneralDTO
            {
                Message = []
            };

            // Validar si el email ya existe
            var emailExiste = await context.Users
                .AnyAsync(u => u.NormalizedEmail == UsuarioDTO.Email.ToUpperInvariant());

            if (emailExiste)
            {
                respuesta.Message.Add("Ya existe una cuenta registrada con este correo electrónico.");
                return BadRequest(respuesta);
            }

            var usuario = mapper.Map<Usuario>(UsuarioDTO);
            usuario.FechaCreacion = DateTime.Now;
            usuario.Activo = true;

            var resultado = await userManager.CreateAsync(usuario, UsuarioDTO.Password!);

            if (resultado.Succeeded)
            {
                // Asignar roles si vienen en la solicitud
                if (UsuarioDTO.Roles.Any())
                {
                    var addRolesResult = await userManager.AddToRolesAsync(usuario, UsuarioDTO.Roles);
                    if (!addRolesResult.Succeeded)
                    {
                        foreach (var error in addRolesResult.Errors)
                        {
                            var customMessage = MapIdentityError(error);
                            respuesta.Message.Add($"[Roles] {customMessage}");
                        }

                        return respuesta;
                    }
                }

                respuesta.Status = true;
                respuesta.Message.Add("Usuario Creado.");
                return respuesta;
            }
            else
            {
                foreach (var error in resultado.Errors)
                {
                    var customMessage = MapIdentityError(error);
                    respuesta.Message.Add(customMessage);
                }

                return respuesta;
            }
        }

        [HttpGet("Detalles/{userName}")]
        [Authorize]
        [EndpointSummary("Obtiene los detalles de un usuario por su UserName.")]
        public async Task<ActionResult<RespuestaObjetoDTO>> GetUsuarioPorUserName(string userName)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var usuario = await userManager.FindByNameAsync(userName);

            if (usuario is null)
            {
                respuesta.Message.Add("Usuario no encontrado.");
                return NotFound(respuesta);
            }

            var usuarioDTO = mapper.Map<UsuarioDTO>(usuario);
            usuarioDTO.Roles = await (from ur in context.UserRoles
                                      join r in context.Roles on ur.RoleId equals r.Id
                                      where ur.UserId == usuario.Id
                                      select r.Name).ToListAsync();

            respuesta.Status = true;
            respuesta.Message.Add("Detalles del usuario recuperados.");
            respuesta.Response = usuarioDTO;

            return respuesta;
        }

        [HttpPost("Login")]
        [EndpointSummary("Obtiene token al ingresar las credenciales correctas")]
        public async Task<ActionResult<RespuestaObjetoDTO>> Login(
            CredencialesUsuarioDTO credencialesUsuarioDTO)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };
            try
            {
                var usuario = await userManager.FindByNameAsync(credencialesUsuarioDTO.UserName!);
                //FindByEmailAsync(credencialesUsuarioDTO.Email);

                if (usuario is null)
                {
                    respuesta.Message.Add("El usuario no se encuentra.");
                    return respuesta;
                }

                var resultado = await signInManager.CheckPasswordSignInAsync(usuario,
                    credencialesUsuarioDTO.Password!, lockoutOnFailure: false);

                var UsuarioDTOCompleto = mapper.Map<UsuarioDTO>(usuario);

                if (resultado.Succeeded)
                {
                    var token = await ConstruirToken(UsuarioDTOCompleto);
                    respuesta.Status = true;
                    respuesta.Message.Add("Login correcto.");
                    respuesta.Response = token;
                    return respuesta;
                }
                else
                {
                    respuesta.Message.Add("Revise el usuario o contraseña e intente de nuevo.");
                    return respuesta;
                }
            }
            catch (Exception ex)
            {
                respuesta.Message.Add(ex.Message);
                return respuesta;
            }
        }

        [HttpPut("Actualizar/{userName}")]
        [Authorize]
        [EndpointSummary("Actualiza datos de Usuario. Si la contraseña viene llena se cambia, si viene vacía no se hace nada. También sincroniza roles.")]
        public async Task<ActionResult<RespuestaObjetoDTO>> UpdateUsuario(ActualizaUsuarioDTO actualizarUsuarioDTO, string userName)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var usuario = await userManager.FindByNameAsync(userName);

            if (usuario is null)
            {
                respuesta.Message.Add("El usuario no fue encontrado.");
                return respuesta;
            }

            // Validar si el email ya existe en otro usuario
            if (!string.IsNullOrWhiteSpace(actualizarUsuarioDTO.Email))
            {
                var emailExisteEnOtroUsuario = await context.Users
                    .AnyAsync(u => u.NormalizedEmail == actualizarUsuarioDTO.Email.ToUpperInvariant()
                                && u.Id != usuario.Id);

                if (emailExisteEnOtroUsuario)
                {
                    respuesta.Message.Add("Ya existe otra cuenta registrada con este correo electrónico.");
                    return BadRequest(respuesta);
                }
            }

            // Actualiza datos básicos
            usuario.Nombre = actualizarUsuarioDTO.FirstName;
            usuario.Apellidos = actualizarUsuarioDTO.LastName;
            usuario.Email = actualizarUsuarioDTO.Email;
            usuario.Activo = actualizarUsuarioDTO.Activo;

            var updateResult = await userManager.UpdateAsync(usuario);

            if (!updateResult.Succeeded)
            {
                respuesta.Message.Add("Error al actualizar datos del usuario.");
                return respuesta;
            }

            // Actualizar contraseña si se proporciona
            if (!string.IsNullOrWhiteSpace(actualizarUsuarioDTO.NewPassword))
            {
                var removeResult = await userManager.RemovePasswordAsync(usuario);
                if (!removeResult.Succeeded)
                {
                    respuesta.Message.Add("No se pudo remover la contraseña anterior.");
                    return respuesta;
                }

                var addResult = await userManager.AddPasswordAsync(usuario, actualizarUsuarioDTO.NewPassword);
                if (!addResult.Succeeded)
                {
                    respuesta.Message.Add("No se pudo asignar la nueva contraseña.");
                    return respuesta;
                }

                respuesta.Message.Add("Contraseña actualizada.");
            }

            // Sincronizar roles
            if (actualizarUsuarioDTO.Roles != null)
            {
                var rolesActuales = await userManager.GetRolesAsync(usuario);
                var rolesAAgregar = actualizarUsuarioDTO.Roles.Except(rolesActuales).ToList();
                var rolesAEliminar = rolesActuales.Except(actualizarUsuarioDTO.Roles).ToList();
                if (rolesAAgregar.Any())
                {
                    var addRolesResult = await userManager.AddToRolesAsync(usuario, rolesAAgregar);
                    if (!addRolesResult.Succeeded)
                    {
                        foreach (var error in addRolesResult.Errors)
                            respuesta.Message.Add($"[Agregar Rol] {error.Description}");
                    }
                }
                if (rolesAEliminar.Any())
                {
                    var removeRolesResult = await userManager.RemoveFromRolesAsync(usuario, rolesAEliminar);
                    if (!removeRolesResult.Succeeded)
                    {
                        foreach (var error in removeRolesResult.Errors)
                            respuesta.Message.Add($"[Eliminar Rol] {error.Description}");
                    }
                }
                respuesta.Message.Add("Roles actualizados.");
            }

            respuesta.Status = true;
            respuesta.Message.Add("Datos del usuario actualizados.");
            return respuesta;
        }

        [HttpDelete("Eliminar/{username}")]
        [Authorize]
        [EndpointSummary("Elimina un usuario por su nombre de usuario (UserName)")]
        public async Task<ActionResult<RespuestaObjetoDTO>> EliminarUsuario(string username)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var usuario = await userManager.FindByNameAsync(username);
            if (usuario == null)
            {
                respuesta.Message.Add("Usuario no encontrado.");
                return respuesta;
            }

            var result = await userManager.DeleteAsync(usuario);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    respuesta.Message.Add(error.Description);
                }
                return respuesta;
            }

            respuesta.Status = true;
            respuesta.Message.Add("Usuario eliminado correctamente.");
            respuesta.Response = username;
            return respuesta;
        }

        [HttpGet("renovar-token")]
        [EndpointSummary("Renueva token de un usuario")]
        public async Task<ActionResult<RespuestaObjetoDTO>> RenovarToken()
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var usuario = await serviciosUsuarios.ObtenerUsuario();

            if (usuario is null)
            {
                respuesta.Message.Add("No se puede renovar un token ya expirado");
                return respuesta;
            }

            var UsuarioDTOCompleto = mapper.Map<UsuarioDTO>(usuario);

            var respuestaAutenticacion = await ConstruirToken(UsuarioDTOCompleto);

            respuesta.Status = true;
            respuesta.Message.Add("Token Renovado");
            respuesta.Response = respuestaAutenticacion;

            return respuesta;
        }

        private async Task<RespuestaAutenticacionDTO> ConstruirToken(
            UsuarioDTO credencialesUsuarioDTO)
        {
            var claims = new List<Claim>
            {
                new("FirstName", credencialesUsuarioDTO.FirstName!),
                new("LastName", credencialesUsuarioDTO.LastName!),
                new("Email", credencialesUsuarioDTO.Email!),
                new("UserName",credencialesUsuarioDTO.UserName!)
            };
            var usuario = await userManager.FindByNameAsync(credencialesUsuarioDTO.UserName!);
            var roles = await userManager.GetRolesAsync(usuario!);
            foreach (var rol in roles)
            {
                claims.Add(new Claim("Roles", rol));
            }

            //var claimsDB = await userManager.GetClaimsAsync(usuario!);

            //claims.AddRange(claimsDB);

            var llave = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(configuration["LlaveJWT"]!));
            var credemciales = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

            var expiracion = DateTime.UtcNow.AddMinutes(2);

            var TokenSeguridad = new JwtSecurityToken(issuer: null, audience: null,
                claims: claims, expires: expiracion, signingCredentials: credemciales);

            var token = new JwtSecurityTokenHandler().WriteToken(TokenSeguridad);

            return new RespuestaAutenticacionDTO
            {
                Token = token,
                Expiracion = expiracion
            };
        }

        [HttpPost("forgotpassword")]
        [EndpointSummary("Genera un token para restablecer la contraseña del usuario y lo envía por correo")]
        public async Task<ActionResult<RespuestaObjetoDTO>> ForgotPassword(ForgotPasswordDTO model, [FromServices] IEmailService emailService)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                respuesta.Message.Add("El correo no se encuentra registrado.");
                return NotFound(respuesta);
            }

            // Generar token
            var token = await userManager.GeneratePasswordResetTokenAsync(user);

            // Codificar el token y email para URL
            var encodedToken = Uri.EscapeDataString(token);
            //var encodedEmail = Uri.EscapeDataString(model.Email);
            var encodedEmail = model.Email;

            // Construir link hacia el frontend con parámetros en query string
            var resetLink = $"https://convenciones.clickseguros.lat/auth/resetpassword?token={encodedToken}&email={encodedEmail}";

            // Construir mensaje HTML
            var mensajeHtml = $@"
        <h2>Recuperación de contraseña</h2>
        <p>Hola, {user.UserName}:</p>
        <p>Hemos recibido una solicitud para restablecer tu contraseña.</p>
        <p>Da clic en el siguiente enlace para continuar:</p>
        <p><a href='{resetLink}' target='_blank'>Restablecer contraseña</a></p>
        <p>Si no solicitaste este cambio, ignora este mensaje.</p>
        <hr>
        <p style='font-size:12px;color:#888;'>Este enlace es válido por tiempo limitado.</p>
    ";

            // Enviar correo
            await emailService.SendEmailAsync(model.Email, "Recupera tu contraseña", mensajeHtml);

            respuesta.Status = true;
            respuesta.Message.Add("Se ha enviado un correo con el enlace para restablecer tu contraseña.");
            return Ok(respuesta);
        }

        [HttpPost("resetpassword")]
        [EndpointSummary("Permite al usuario cambiar su contraseña usando un token válido")]
        public async Task<ActionResult<RespuestaObjetoDTO>> ResetPassword(ResetPasswordDTO model)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                respuesta.Message.Add("Correo no encontrado.");
                return NotFound(respuesta);
            }

            var result = await userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);

            if (!result.Succeeded)
            {
                respuesta.Message.AddRange(result.Errors.Select(e => e.Description));
                return BadRequest(respuesta);
            }

            respuesta.Status = true;
            respuesta.Message.Add("La contraseña se ha restablecido correctamente.");
            return Ok(respuesta);
        }
        private string MapIdentityError(IdentityError error)
        {
            switch (error.Code)
            {
                case "DuplicateUserName":
                    return "Este nombre de usuario ya está en uso.";
                case "DuplicateEmail":
                    return "Ya existe una cuenta registrada con este correo electrónico.";
                case "PasswordTooShort":
                    return "La contraseña es demasiado corta.";
                case "PasswordRequiresNonAlphanumeric":
                    return "La contraseña debe contener al menos un carácter no alfanumérico.";
                case "PasswordRequiresDigit":
                    return "La contraseña debe contener al menos un número.";
                case "PasswordRequiresUpper":
                    return "La contraseña debe contener al menos una letra mayúscula.";
                case "PasswordRequiresLower":
                    return "La contraseña debe contener al menos una letra minúscula.";
                default:
                    return error.Description; // Por defecto, usa el mensaje original
            }
        }

    }
}
