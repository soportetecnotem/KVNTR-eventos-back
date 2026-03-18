namespace EventosBack.DTO.Encuesta
{
    public class EncuestaDTO
    {
        public int EventoId { get; set; }
        public List<int> PreguntasId { get; set; } = [];
    }
}
