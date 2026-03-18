namespace EventosBack.Models
{
    public class Categoria_Recomendacion
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public bool Activo { get; set; }
        //Propiedad de navegacion para propiedad recomendaciones -- una categoria a muchas recomendaciones
        public List<Recomendacion> Recomendaciones { get; set; } = [];
    }
}
