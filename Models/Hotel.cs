namespace EventosBack.Models
{
    public class Hotel
    {
        public int Id { get; set; }
        public string NombreHotel { get; set; } = null!;
        public string Telefono { get; set; } = null!;
        public string Direccion { get; set; } = null!;
        public string Latitud { get; set; } = null!;
        public string Longitud { get; set; } = null!;
        public string Imagen { get; set; } = null!;
        public string Detalles { get; set; } = null!;

        //Propiedad de navegacion para vincular a un evento -- un evento muchos hoteles
        public int EventoId { get; set; }
        public Evento? Evento { get; set; }

        //Propiedad de navegacion para relacion con Usuarios --Muchos a Muchos
        public List<HotelUsuario> Convencionistas { get; set; } = [];
    }
}
