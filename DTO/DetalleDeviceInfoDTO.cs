namespace EventosBack.DTO
{
    public class DetalleDeviceInfoDTO : DeviceInfoDTO
    {
        public string Platform { get; set; } = null!;
        public string Version { get; set; } = null!;
        public int Status { get; set; }
        public DateTime F_Registo { get; set; }
        public DateTime F_Update { get; set; }
    }
}
