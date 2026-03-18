using EventosBack.Data;
using EventosBack.DTO;
using EventosBack.DTO.Responses;
using EventosBack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace EventosBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MicrosoftAuthController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        public static byte[] _key = null!;

        public MicrosoftAuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            var keyString = configuration.GetValue<string>("Key_Microsoft");
            _key = Encoding.UTF8.GetBytes(keyString!);
            this.context = context;
        }

        [HttpPost("GuardarCredenciales")]
        [EndpointSummary("Guarda o actualiza credenciales encriptadas de Microsoft")]
        public async Task<ActionResult<RespuestaObjetoDTO>> GuardarCredencialesAsync(AuthMicrosoft input)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            try
            {
                // Encriptar campos
                var entity = new AuthMicrosoft
                {
                    tenantId = CryptoHelper.Encriptar(input.tenantId, _key),
                    clientId = CryptoHelper.Encriptar(input.clientId, _key),
                    scope = CryptoHelper.Encriptar(input.scope, _key),
                    grantType = CryptoHelper.Encriptar(input.grantType, _key),
                    clientSecret = CryptoHelper.Encriptar(input.clientSecret, _key),
                    userId = CryptoHelper.Encriptar(input.userId, _key),
                    email = input.email
                };

                // Verificar si ya existe por email
                var existente = await context.Set<AuthMicrosoft>()
                    .FirstOrDefaultAsync(x => x.email == input.email);

                if (existente != null)
                {
                    // Actualizar campos
                    existente.tenantId = entity.tenantId;
                    existente.clientId = entity.clientId;
                    existente.scope = entity.scope;
                    existente.grantType = entity.grantType;
                    existente.clientSecret = entity.clientSecret;
                    existente.userId = entity.userId;

                    context.Update(existente);
                }
                else
                {
                    // Insertar nuevo
                    context.Add(entity);
                }

                await context.SaveChangesAsync();

                respuesta.Status = true;
                respuesta.Message.Add("Credenciales guardadas correctamente.");
                respuesta.Response = entity; // O entity si deseas devolver lo encriptado
                return Ok(respuesta);
            }
            catch (Exception ex)
            {
                respuesta.Status = false;
                respuesta.Message.Add("Error al guardar credenciales: " + ex.Message);
                return BadRequest(respuesta);
            }
        }

        //[HttpPost("ConsultaCredenciales")]
        //[EndpointSummary("Obtiene las credenciales de Microsoft desencriptadas")]
        private async Task<AuthMicrosoft> ObtenerCredencialesDesencriptadasAsync(string email)
        {
            var record = await context.Set<AuthMicrosoft>().FirstOrDefaultAsync(x => x.email == email);
            return record == null
                ? throw new Exception("Credenciales no encontradas.")
                : new AuthMicrosoft
                {
                    tenantId = CryptoHelper.Desencriptar(record.tenantId, _key),
                    clientId = CryptoHelper.Desencriptar(record.clientId, _key),
                    scope = CryptoHelper.Desencriptar(record.scope, _key),
                    grantType = CryptoHelper.Desencriptar(record.grantType, _key),
                    clientSecret = CryptoHelper.Desencriptar(record.clientSecret, _key),
                    userId = record.userId,
                    email = record.email
                };
        }

        [HttpPost("ObtenerCredenciales")]
        [EndpointSummary("Obtiene las credenciales de Microsoft desencriptadas desde base")]
        public async Task<ActionResult<RespuestaObjetoDTO>> ObtenerCredencialesMicrosoft(GetAuthMicrosoft info)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var auth = await ObtenerCredencialesDesencriptadasAsync(info.email);

            if (auth != null)
            {
                respuesta.Status = true;
                respuesta.Message.Add("Ok");
                respuesta.Response = auth;
            }
            else
            {
                respuesta.Message.Add("No se encontro informacion");
            }
            return respuesta;
        }

        [HttpPost("ObtenerToken")]
        [EndpointSummary("Obtiene un token OAuth2 Microsoft")]
        public async Task<ActionResult<RespuestaObjetoDTO>> ObtenerTokenDesdeAzureAsync(GetAuthMicrosoft info)
        {
            var respuesta = new RespuestaObjetoDTO
            {
                Message = []
            };

            var auth = await ObtenerCredencialesDesencriptadasAsync(info.email);
            var url = $"https://login.microsoftonline.com/{auth.tenantId}/oauth2/v2.0/token";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //client.DefaultRequestHeaders.Add("Cookie", "fpc=xxx; stsservicecookie=estsfd; x-ms-gateway-slice=estsfd");

            var form = new Dictionary<string, string>
            {
                {"client_id", auth.clientId},
                {"scope", auth.scope},
                {"grant_type", auth.grantType},
                {"client_secret", auth.clientSecret}
            };

            var content = new FormUrlEncodedContent(form);
            var response = await client.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                respuesta.Message.Add($"Error al obtener token: {response.StatusCode}");
                respuesta.Response = error;
                return respuesta;
            }

            var json = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<ResponseAuthMicrosoft>(json);
            if (tokenResponse != null)
            {
                respuesta.Status = true;
                respuesta.Message.Add("Ok");
                respuesta.Response = tokenResponse;
            }
            else
            {
                respuesta.Message.Add("Token Null");
            }
            return respuesta;
        }
    }

    // Clase para encriptar y desencriptar (simple AES)
    public static class CryptoHelper
    {
        public static string Encriptar(string texto, byte[] _key)
        {

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            ms.Write(aes.IV, 0, aes.IV.Length); // Guardar IV al inicio del stream

            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(texto);
            }

            return Convert.ToBase64String(ms.ToArray());
        }
        public static string Desencriptar(string textoEncriptado, byte[] _key)
        {

            var datos = Convert.FromBase64String(textoEncriptado);

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = datos.Take(16).ToArray();

            using var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream(datos.Skip(16).ToArray());
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);

            return sr.ReadToEnd();
        }
    }
}
