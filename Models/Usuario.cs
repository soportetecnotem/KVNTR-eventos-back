using Microsoft.AspNetCore.Identity;

namespace EventosBack.Models
{
    public class Usuario : IdentityUser
    {
        public string? Nombre { get; set; }
        public string? Apellidos { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool Activo { get; set; }
    }
}
