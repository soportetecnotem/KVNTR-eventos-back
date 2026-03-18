namespace EventosBack.DTO
{
    public class ConvencionistaDTO
    {
        public int Id { get; set; }
        public string Clave { get; set; } = null!;
        public string NombreCompleto { get; set; } = null!;
        public string Puesto { get; set; } = null!;
        public string? Telefono { get; set; }
        public string Imagen { get; set; } = null!;
        public string Documento { get; set; } = null!;
        public bool Activo { get; set; }
    }
}
