namespace EventosBack.Models
{
    public class PreguntaEvento
    {
        //Propiedad de navegacion para vincular con pregunta
        public int PreguntaId { get; set; }
        public Pregunta? Pregunta { get; set; }

        //Propiedad de navegacion para vincular con Evento      
        public int EventoId { get; set; }
        public Evento? Evento { get; set; }
    }
}
