namespace EventosBack.Models
{
    public class DeviceInfo
    {
        public int Id { get; set; }
        public string Cve_Vend { get; set; } = null!;
        public string Token_ID { get; set; } = null!;
        public string ID_Device { get; set; } = null!;
        public int Status { get; set; }
        public DateTime F_Registo { get; set; }
        public DateTime F_Update { get; set; }
        public string Platform { get; set; } = null!;
        public string Version { get; set; } = null!;
        public string Manufactura { get; set; } = null!;
    }
}
