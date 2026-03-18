namespace EventosBack.Models
{
    public class Actividad
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = null!;
        public string Subtitulo { get; set; } = null!;
        public string Especificaciones { get; set; } = null!;
        public string Imagen { get; set; } = null!;
        public DateTime Fecha { get; set; }

        //Propiedad de navegacion para vincular a un evento -- un evento muchos actividades
        public int EventoId { get; set; }
        public Evento? Evento { get; set; }

        //Propiedad de navegacion para vincular a una categoria
        public int Categoria_ActividadesId { get; set; }
        public Categoria_Actividades? Categoria { get; set; }

        //Propiedad de navegacion para relacion con Usuarios --Muchos a Muchos
        public List<ActividadUsuario> Convencionistas { get; set; } = [];
    }
}
