namespace EventosBack.Models
{
    public class Recomendacion
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = null!;
        public string Informacion { get; set; } = null!;
        public string Latitud { get; set; } = null!;
        public string Longitud { get; set; } = null!;
        public string Imagen { get; set; } = null!;

        //Propiedad de navegacion para vincular a un evento
        public int EventoId { get; set; }
        public Evento? Evento { get; set; }

        //Propiedad de navegacion para vincular a una categoria de recomendacion
        public int Categoria_RecomendacionId { get; set; }
        public Categoria_Recomendacion? CategoriaRecomendacion { get; set; }
    }
}
