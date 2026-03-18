namespace EventosBack.DTO
{
    public class ActualizarVueloDTO : VueloDTO
    {
        public IEnumerable<int> ConvencionistasIds { get; set; } = [];
    }
}
