using System.ComponentModel.DataAnnotations;

namespace EventosBack.DTO
{
    public class EditarClaimDTO
    {
        [Required]
        public required string UserName { get; set; }
    }
}
