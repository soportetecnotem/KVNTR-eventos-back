namespace EventosBack.DTO
{
    public class CreateRecomendacionDTO
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = null!;
        public string Informacion { get; set; } = null!;
        public string Latitud { get; set; } = null!;
        public string Longitud { get; set; } = null!;
        public string Imagen { get; set; } = null!;
        public int EventoId { get; set; }
        public int Categoria_RecomendacionId { get; set; }
    }
}
