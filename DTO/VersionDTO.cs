namespace EventosBack.DTO
{
    public class VersionDTO
    {
        public int Id { get; set; }
        public string Version_Android { get; set; } = null!;
        public string Version_IOs { get; set; } = null!;
        public string Version_Huawei { get; set; } = null!;
    }
}
