namespace EventosBack.Models
{
    public class AuthMicrosoft
    {
        public int Id { get; set; }
        public string tenantId { get; set; } = null!;
        public string clientId { get; set; } = null!;
        public string scope { get; set; } = null!;
        public string grantType { get; set; } = null!;
        public string clientSecret { get; set; } = null!;
        public string userId { get; set; } = null!;
        public string email { get; set; } = null!;
    }
}
