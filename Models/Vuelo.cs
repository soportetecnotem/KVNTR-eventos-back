namespace EventosBack.Models
{
    public class Vuelo
    {
        public int Id { get; set; }
        public DateTime Fecha_Vuelo { get; set; }
        public string Reservacion { get; set; } = null!;
        public string Numero_Vuelo { get; set; } = null!;
        public string Asiento { get; set; } = null!;
        public string Origen { get; set; } = null!;
        public string Lugar_Origen { get; set; } = null!;
        public string Hora_Salida { get; set; } = null!;
        public string Destino { get; set; } = null!;
        public string Lugar_Destino { get; set; } = null!;
        public string Hora_Llegada { get; set; } = null!;
        public string Detalle { get; set; } = null!;

        //Propiedad de navegacion para vincular a un evento
        public int EventoId { get; set; }
        public Evento? Evento { get; set; }

        //Propiedad de navegacion para relacion con Usuarios --Un vuelo a Muchos convencionistas
        public List<VueloUsuario> VuelosUsuarios { get; set; } = [];
    }
}
