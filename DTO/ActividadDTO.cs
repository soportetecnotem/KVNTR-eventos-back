namespace EventosBack.DTO
{
    public class ActividadDTO
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = null!;
        public string Subtitulo { get; set; } = null!;
        public string Especificaciones { get; set; } = null!;
        public string Imagen { get; set; } = null!;
        public DateTime Fecha { get; set; }
        public int EventoId { get; set; }
        public int Categoria_ActividadesId { get; set; }
        public IEnumerable<int> ConvencionistasIds { get; set; } = [];
    }
}
