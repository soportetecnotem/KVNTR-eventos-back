using Microsoft.EntityFrameworkCore;

namespace EventosBack.Models
{
    //Tabla puente para relacion muchos a muchos
    [PrimaryKey(nameof(HotelId), nameof(ConvencionistaId))]
    public class HotelUsuario
    {
        //Propiedad de navegacion para vincular con el hotel
        public int HotelId { get; set; }
        public Hotel? Hotel { get; set; }

        //Propiedad de navegacion para vincular con el usuario        
        public int ConvencionistaId { get; set; }
        public Convencionista? Convencionista { get; set; }
    }
}
