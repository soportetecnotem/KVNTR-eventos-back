using Microsoft.EntityFrameworkCore;

namespace EventosBack.Models
{
    //Tabla puente para relacion muchos a muchos
    [PrimaryKey(nameof(EventoId), nameof(ConvencionistaId))]
    public class EventoUsuario
    {
        //Propiedad de navegacion para vincular con el hotel
        public int EventoId { get; set; }
        public Evento? Evento { get; set; }

        //Propiedad de navegacion para vincular con el usuario
        public int ConvencionistaId { get; set; }
        public Convencionista? Convencionista { get; set; }
        public bool Activo { get; set; }
    }
}
