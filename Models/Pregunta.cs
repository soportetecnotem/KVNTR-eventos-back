namespace EventosBack.Models
{
    public class Pregunta
    {
        public int Id { get; set; }
        public string Texto { get; set; } = null!;

        //Listado de preguntas que pertenecen a esta encuesta
        public List<Respuesta> Respuestas { get; set; } = null!;
    }
}
