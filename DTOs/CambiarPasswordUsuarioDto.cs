using System.ComponentModel.DataAnnotations;

namespace SICOP.DTOs
{
    // Recibe la nueva contraseña de un usuario.
    public class CambiarPasswordUsuarioDto
    {
        // La contraseña debe tener al menos 8 caracteres.
        [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        public string NuevaPassword { get; set; } = string.Empty;
    }
}