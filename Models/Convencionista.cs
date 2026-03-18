namespace EventosBack.Models
{
    public class Convencionista
    {
        public int Id { get; set; }
        public string Clave { get; set; } = null!;
        public string NombreCompleto { get; set; } = null!;
        public string Puesto { get; set; } = null!;
        public string Telefono { get; set; } = null!;
        public string Imagen { get; set; } = null!;
        public string Documento { get; set; } = null!;
        public bool Activo { get; set; }

        //Propiedad de navegacion para vincular a un evento -- muchos a muchos
        public List<EventoUsuario> EventosUsuarios { get; set; } = [];

        //Propiedad de navegacion para vincular con los hoteles -- muchos a muchos
        public List<HotelUsuario> HotelesUsuarios { get; set; } = [];

        //Propiedad de navegacion para vincular con los Vuelos -- muchos a muchos
        public List<VueloUsuario> VuelosUsuarios { get; set; } = [];

        //Propiedad de navegacion para vincular a un perfil de usuario -- uno a muchos 
        //Un perfil puede aplicar en varios convencionistas, un convencionista tiene solo un perfil
        public int PerfilConvencionistaId { get; set; }
        public PerfilConvencionista? PerfilConvencionista { get; set; }

        //Propiedad de navegacion para vincular a una categoria de usuario -- uno a muchos
        public int CategoriaUsuarioId { get; set; }
        public CategoriaUsuario? CategoriaUsuario { get; set; }
    }
}
