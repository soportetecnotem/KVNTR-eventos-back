using Microsoft.EntityFrameworkCore;

namespace EventosBack.Models
{
    //Tabla puente para relacion muchos a muchos
    [PrimaryKey(nameof(VueloId), nameof(ConvencionistaId))]
    public class VueloUsuario
    {
        //Propiedad de navegacion para vincular con el hotel
        public int VueloId { get; set; }
        public Vuelo? Vuelo { get; set; }

        //Propiedad de navegacion para vincular con el usuario        
        public int ConvencionistaId { get; set; }
        public Convencionista? Convencionista { get; set; }
    }
}
