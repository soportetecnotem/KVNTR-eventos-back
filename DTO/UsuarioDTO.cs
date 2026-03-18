using System.ComponentModel.DataAnnotations;

namespace EventosBack.DTO
{
    public class UsuarioDTO
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        public required string UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido.")]
        [Required(ErrorMessage = "El email es obligatorio.")]
        public required string Email { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool Activo { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}
