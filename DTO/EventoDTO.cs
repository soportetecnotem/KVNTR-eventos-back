namespace EventosBack.DTO
{
    public class EventoDTO
    {
        public int Id { get; set; }
        public required string NombreEvento { get; set; }
        public string Subtitulo { get; set; } = null!;
        public bool Activo { get; set; }
        public DateTime Fecha_inicio { get; set; }
        public DateTime Fecha_fin { get; set; }
        public string Imagen { get; set; } = null!;
        public string Direccion { get; set; } = null!;
        public string Latitud { get; set; } = null!;
        public string Longitud { get; set; } = null!;
        public string LugarDestino { get; set; } = null!;
    }
}
