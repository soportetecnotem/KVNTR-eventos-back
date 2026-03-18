namespace EventosBack.Models
{
    public class Respuesta
    {
        public int Id { get; set; }
        public int Calificacion { get; set; }
        public string Comentario { get; set; } = null!;

        //Propiedad de navegacion para vincular con la respuesta
        public int PreguntaId { get; set; }
        public Pregunta? Pregunta { get; set; }

        //Propiedad de navegacion para vincular con el usuario
        public int ConvencionistaId { get; set; }
        public Convencionista? Convencionista { get; set; }

        //Propiedad de navegacion para vincular con Evento      
        public int EventoId { get; set; }
        public Evento? Evento { get; set; }
    }
}
