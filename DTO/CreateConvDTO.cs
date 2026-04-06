namespace EventosBack.DTO
{
    public class CreateConvDTO : ConvencionistaDTO
    {
        public int PerfilId { get; set; }
        public int CategoriaId { get; set; }
        public int EventoId { get; set; }
        public string Contrasena { get; set; } = string.Empty;
    }
}