namespace EventosBack.Models
{
    public class Evento
    {
        public int Id { get; set; }
        public string NombreConvencion { get; set; } = null!;
        public string Subtitulo { get; set; } = null!;
        public bool Activo { get; set; }
        public DateTime Fecha_inicio { get; set; }
        public DateTime Fecha_fin { get; set; }
        public string Imagen { get; set; } = null!;
        public string Direccion { get; set; } = null!;
        public string Latitud { get; set; } = null!;
        public string Longitud { get; set; } = null!;
        public string LugarDestino { get; set; } = null!;

        //Propiedad de navegacion para propiedad hoteles -- un evento a muchos hoteles
        public List<Hotel> Hoteles { get; set; } = [];

        //Propiedad de navegacion para propiedad Vuelos -- un evento a muchos vuelos
        public List<Vuelo> Vuelos { get; set; } = [];

        //Propiedad de navegacion para relacion con Usuarios --Muchos a Muchos
        public List<EventoUsuario> Convencionistas { get; set; } = [];
    }
}
