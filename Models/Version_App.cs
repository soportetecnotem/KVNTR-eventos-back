namespace EventosBack.Models
{
    public class Version_App
    {
        public int Id { get; set; }
        public string Version_Android { get; set; } = null!;
        public string Version_IOs { get; set; } = null!;
        public string Version_Huawei { get; set; } = null!;
        public DateTime Fecha { get; set; }
    }
}
