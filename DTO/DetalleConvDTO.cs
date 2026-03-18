namespace EventosBack.DTO
{
    public class DetalleConvDTO : ConvencionistaDTO
    {
        public int PerfilId { get; set; }
        public string PerfilNombre { get; set; } = null!;
        public int CategoriaId { get; set; }
        public string CategoriaNombre { get; set; } = null!;
        public int EventoId { get; set; }
        public string NombreEvento { get; set; } = null!;
    }
}
