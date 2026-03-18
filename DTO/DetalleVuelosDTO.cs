namespace EventosBack.DTO
{
    public class DetalleVuelosDTO : VueloDTO
    {
        public int Id { get; set; }
        public string NombreEvento { get; set; } = null!;
        public IEnumerable<int> ConvencionistasIds { get; set; } = [];
    }
}
