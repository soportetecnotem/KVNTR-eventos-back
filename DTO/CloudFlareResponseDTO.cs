namespace EventosBack.DTO
{
    public class CloudFlareResponseDTO
    {
        public Result result { get; set; }
        public bool success { get; set; }
        public object[] errors { get; set; }
        public object[] messages { get; set; }
    }

    public class Result
    {
        public string id { get; set; }
        public string filename { get; set; }
        public DateTime uploaded { get; set; }
        public bool requireSignedURLs { get; set; }
        public string[] variants { get; set; }
    }

}