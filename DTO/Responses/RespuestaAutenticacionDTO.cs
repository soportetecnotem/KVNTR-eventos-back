namespace EventosBack.DTO.Responses
{
    public class RespuestaAutenticacionDTO
    {
        public string? Token { get; set; }
        public DateTime Expiracion { get; set; }
    }
}
