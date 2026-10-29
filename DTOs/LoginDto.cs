using System.ComponentModel.DataAnnotations;

namespace SICOP.DTOs
{
    // Define los datos necesarios para iniciar sesión.
    public class LoginDto
    {
        // Correo electrónico del usuario.
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
        public string Correo { get; set; } = string.Empty;

        // Contraseña del usuario.
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public string Password { get; set; } = string.Empty;
    }
}