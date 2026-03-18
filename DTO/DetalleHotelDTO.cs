namespace EventosBack.DTO
{
    public class DetalleHotelDTO : HotelDTO
    {
        public int Id { get; set; }
        public string NombreEvento { get; set; } = null!;
    }
}
