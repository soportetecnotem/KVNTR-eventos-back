namespace EventosBack.DTO
{
    public class RecomendacionDTO : CreateRecomendacionDTO
    {
        public string NombreEvento { get; set; } = null!;
        public string NombreCategoriaReco { get; set; } = null!;
    }
}
