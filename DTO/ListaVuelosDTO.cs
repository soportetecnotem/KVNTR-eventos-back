namespace EventosBack.DTO
{
    public class ListaVuelosDTO : VueloDTO
    {
        public int Id { get; set; }
        public string NombreEvento { get; set; } = null!;
    }
}
