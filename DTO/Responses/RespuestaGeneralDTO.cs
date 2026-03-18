namespace EventosBack.DTO.Responses
{
    public class RespuestaGeneralDTO
    {
        public bool Status { get; set; }
        public List<string> Message { get; set; } = new List<string>();
    }
}
