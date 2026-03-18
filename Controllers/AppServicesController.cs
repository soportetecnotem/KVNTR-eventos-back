using AutoMapper;
using EventosBack.Data;
using EventosBack.DTO;
using EventosBack.DTO.Responses;
using EventosBack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    [Authorize]
    public class AppServicesController : ControllerBase
    {
        //private readonly ISicasService _sicasService;
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IConfiguration configuration;

        public AppServicesController( ApplicationDbContext context, IMapper mapper, IConfiguration configuration)
        {
            //this._sicasService = sicasService;
            this.context = context;
            this.mapper = mapper;
            this.configuration = configuration;
        }

        //Login
        [HttpPost("Validar")]
        [AllowAnonymous]
        [EndpointSummary("Valida usuario y contraseña de agentes en SICAS y retorna token para iniciar sesión en App")]
        public async Task<ActionResult<RespuestaObjetoDTO>> Validar([FromBody] LoginModels paramsModel)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            try
            {
                // 1. Buscar al convencionista primero
                var convencionista = await context.Convencionistas
                    .Include(x => x.PerfilConvencionista)
                    .Include(x => x.CategoriaUsuario)
                    .Include(x => x.EventosUsuarios)
                        .ThenInclude(x => x.Evento)
                    .Where(c => c.EventosUsuarios.Any(eu => eu.Activo)) // Solo eventos activos
                    .FirstOrDefaultAsync(c => c.Clave == paramsModel.Cve_Vend);

                if (convencionista is null)
                {
                    // No existe convencionista
                    respuesta.Status = false;
                    respuesta.Message.Add("Convencionista no encontrado");
                    return Ok(respuesta);
                }

                //// 2. Validar si es usuario SICAS
                //var result = await _sicasService.ValidaAccesoSicasAsync(paramsModel.Cve_Vend, paramsModel.Contrasena);

                // Mapear convencionista
                var ConvDTO = mapper.Map<DetalleConvDTO>(convencionista);
                var token = await TokenApp(ConvDTO);

                //if (result.Status == true)
                //{
                //    // Es usuario SICAS
                //    respuesta.Status = true;
                //    respuesta.Message.Add("Convencionista Usuario SICAS");
                //    respuesta.Response = new { Access = token, Convencionista = ConvDTO };
                //}
                //else
                //{
                    // Existe como convencionista pero no es usuario SICAS
                    respuesta.Status = true;
                    respuesta.Message.Add("Convencionista Invitado");
                    respuesta.Response = new { Access = token, Convencionista = ConvDTO };
                //}

                return Ok(respuesta);
            }
            catch (Exception ex)
            {
                respuesta.Status = false;
                respuesta.Message.Add(ex.Message);
                return Ok(respuesta);
            }
        }

        //Servicios Version
        [HttpGet("ObtenerVersion")]
        [AllowAnonymous]
        [EndpointSummary("Obtiene la version de la app.")]
        public async Task<ActionResult<RespuestaObjetoDTO>> ObtenerVersion()
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var Versiones = await context.Versiones_App
                .ToListAsync();

            if (Versiones.Count > 0)
            {
                var versionesDTO = mapper.Map<IEnumerable<DetalleVersionDTO>>(Versiones);
                respuesta.Status = true;
                respuesta.Message.Add("Version actual.");
                respuesta.Response = versionesDTO;
                return respuesta;
            }
            else
            {
                respuesta.Message.Add("No se encontró informacion.");
            }
            return respuesta;
        }

        [HttpPut("ActualizarVersion/{id:int}")]
        [EndpointSummary("Actualiza la versión de la app por Id.")]
        public async Task<ActionResult<RespuestaObjetoDTO>> ActualizarVersion(int id, [FromBody] VersionDTO versionDTO)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            // Buscar versión por Id
            var version = await context.Versiones_App.FindAsync(id);

            if (version == null)
            {
                respuesta.Message.Add("No se encontró la versión especificada.");
                return respuesta;
            }

            // Mapear cambios desde DTO al entity
            mapper.Map(versionDTO, version);

            // Actualizar fecha de actualización si existe
            version.Fecha = DateTime.Now;

            try
            {
                context.Versiones_App.Update(version);
                await context.SaveChangesAsync();

                var versionActualizadaDTO = mapper.Map<DetalleVersionDTO>(version);

                respuesta.Status = true;
                respuesta.Message.Add("Versión actualizada correctamente.");
                respuesta.Response = versionActualizadaDTO;
            }
            catch (Exception ex)
            {
                respuesta.Message.Add("Error al actualizar la versión: " + ex.Message);
            }

            return respuesta;
        }


        //Servicios Dispositivos
        [HttpGet("DeviceExists/{IDDevice}")]
        [EndpointSummary("Device_Exists")]
        public async Task<ActionResult<RespuestaObjetoDTO>> Device_Exists(string IDDevice)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var Devices = await context.DevicesInfo
                .Where(x => x.ID_Device == IDDevice)
                .FirstOrDefaultAsync();

            if (Devices != null)
            {
                var devicesDTO = mapper.Map<DeviceInfoDTO>(Devices);
                respuesta.Status = true;
                respuesta.Message.Add("Devices Info");
                respuesta.Response = devicesDTO;
                return respuesta;
            }
            else
            {
                respuesta.Message.Add("No se encontró informacion.");
            }

            return respuesta;
        }

        [HttpPost("GuardarDevice")]
        [EndpointSummary("Agrega o actualiza la información del dispositivo")]
        public async Task<ActionResult<RespuestaObjetoDTO>> GuardarDevice(DetalleDeviceInfoDTO input)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var existente = await context.DevicesInfo
                .FirstOrDefaultAsync(x => x.ID_Device == input.ID_Device);

            if (existente != null)
            {
                // Actualiza campos
                existente.Token_ID = input.Token_ID;
                existente.Cve_Vend = input.Cve_Vend;
                existente.F_Update = DateTime.Now;
                existente.Platform = input.Platform;
                existente.Version = input.Version;
                existente.Manufactura = input.Manufactura;
                existente.Status = input.Status;

                context.DevicesInfo.Update(existente);
                respuesta.Message.Add("Información del dispositivo actualizada.");
                respuesta.Response = mapper.Map<DetalleDeviceInfoDTO>(existente);
            }
            else
            {
                // Nuevo registro
                var nuevo = mapper.Map<DeviceInfo>(input);
                nuevo.F_Registo = DateTime.Now;
                nuevo.F_Update = DateTime.Now;

                context.DevicesInfo.Add(nuevo);
                respuesta.Message.Add("Información del dispositivo registrada.");
                respuesta.Response = mapper.Map<DetalleDeviceInfoDTO>(nuevo);
            }

            await context.SaveChangesAsync();
            respuesta.Status = true;

            return respuesta;
        }

        [HttpPost("CloseDevice")]
        [EndpointSummary("Cierra un dispositivo marcándolo como inactivo (Status = 1)")]
        public async Task<ActionResult<RespuestaObjetoDTO>> CloseDevice([FromBody] DeviceInfoDTO oDevice)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var device = await context.DevicesInfo.FirstOrDefaultAsync(d =>
                d.Cve_Vend == oDevice.Cve_Vend &&
                d.Token_ID == oDevice.Token_ID &&
                d.ID_Device == oDevice.ID_Device);

            if (device == null)
            {
                respuesta.Message.Add("No se encontró el dispositivo con los criterios proporcionados.");
                respuesta.Response = oDevice;
                return respuesta;
            }

            device.Status = 1;
            device.F_Update = DateTime.Now;

            context.DevicesInfo.Update(device);
            await context.SaveChangesAsync();

            respuesta.Status = true;
            respuesta.Message.Add("Dispositivo cerrado correctamente.");
            respuesta.Response = mapper.Map<DetalleDeviceInfoDTO>(device);

            return respuesta;
        }

        //URL POLITICA DE PRIVACIDAD
        [HttpGet("AvisoPrivacidad")]
        [EndpointSummary("Obtiene la URL del aviso de privacidad.")]
        public Task<ActionResult<RespuestaObjetoDTO>> AvisoPrivacidad()
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = [],
                Status = true,
                Response = "https://www.clickseguros.lat/aviso-de-privacidad"
            };

            return Task.FromResult<ActionResult<RespuestaObjetoDTO>>(respuesta);
        }

        private async Task<RespuestaAutenticacionDTO> TokenApp(
            ConvencionistaDTO convencionistaDTO)
        {
            var claims = new List<Claim>
            {
                new("Clave", convencionistaDTO.Clave!),
                new("NombreCompleto", convencionistaDTO.NombreCompleto!)
                //new("PerfilNombreId", credencialesUsuarioDTO.Email!),
                //new("CategoriaNombreId",credencialesUsuarioDTO.UserName!),
                //new("Roles","Sesion App")
            };

            var llave = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(configuration["LlaveJWT"]!));
            var credemciales = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

            var expiracion = DateTime.UtcNow.AddDays(10);

            var TokenSeguridad = new JwtSecurityToken(issuer: null, audience: null,
                claims: claims, expires: expiracion, signingCredentials: credemciales);

            var token = new JwtSecurityTokenHandler().WriteToken(TokenSeguridad);

            return new RespuestaAutenticacionDTO
            {
                Token = token,
                Expiracion = expiracion
            };
        }

    }
}
