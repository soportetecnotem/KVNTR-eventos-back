namespace EventosBack.Models
{
    public class CategoriaUsuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public bool Activo { get; set; }

        //Propiedad de navegacion para propiedad convencionistas -- una categoria a muchos convencionistas
        public List<Convencionista> Convencionistas { get; set; } = [];
    }
}
