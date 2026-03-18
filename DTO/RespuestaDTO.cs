namespace EventosBack.DTO
{
    public class RespuestaDTO
    {
        public int PreguntaId { get; set; }
        public int EventoId { get; set; }
        public int ConvencionistaId { get; set; }
        public string ClaveConvencionista { get; set; } = null!;
        public string NombreConvencionista { get; set; } = null!;
        public string Comentario { get; set; } = null!;
        public int Calificacion { get; set; }
    }
}
