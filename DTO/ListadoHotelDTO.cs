namespace EventosBack.DTO
{
    public class ListadoHotelDTO
    {
        public int Id { get; set; }
        public string NombreHotel { get; set; } = null!;
        public string Telefono { get; set; } = null!;
        public string Direccion { get; set; } = null!;
        public string Latitud { get; set; } = null!;
        public string Longitud { get; set; } = null!;
        public string Imagen { get; set; } = null!;
        public string Detalles { get; set; } = null!;
        public int EventoId { get; set; }
        public string NombreEvento { get; set; } = null!;
    }
}
