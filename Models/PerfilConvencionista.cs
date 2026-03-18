namespace EventosBack.Models
{
    public class PerfilConvencionista
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public bool Activo { get; set; }

        //Propiedad de navegacion para propiedad convencionistas -- un perfil a muchos convencionistas
        public List<Convencionista> Convencionistas { get; set; } = [];
    }
}
