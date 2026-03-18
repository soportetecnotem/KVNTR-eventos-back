namespace EventosBack.DTO
{
    public class CreacionRespuestaDTO
    {
        public int PreguntaId { get; set; }
        public int EventoId { get; set; }
        public int ConvencionistaId { get; set; }
        public string Comentario { get; set; } = null!;
        public int Calificacion { get; set; }
    }
}
