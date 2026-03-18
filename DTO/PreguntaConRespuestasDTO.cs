namespace EventosBack.DTO
{
    public class PreguntaConRespuestasDTO
    {
        public int PreguntaId { get; set; }
        public string Texto { get; set; } = null!;
        public List<RespuestaDTO> Respuestas { get; set; } = [];
    }
}
