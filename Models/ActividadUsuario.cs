using Microsoft.EntityFrameworkCore;

namespace EventosBack.Models
{
    //Tabla puente para relacion muchos a muchos
    [PrimaryKey(nameof(ActividadId), nameof(ConvencionistaId))]
    public class ActividadUsuario
    {
        //Propiedad de navegacion para vincular con el hotel
        public int ActividadId { get; set; }
        public Actividad? Actividad { get; set; }

        //Propiedad de navegacion para vincular con el usuario        
        public int ConvencionistaId { get; set; }
        public Convencionista? Convencionista { get; set; }
    }
}
