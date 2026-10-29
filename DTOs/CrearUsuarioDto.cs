using System.ComponentModel.DataAnnotations;

namespace SICOP.DTOs
{
    // Define los datos necesarios para registrar un usuario.
    public class CrearUsuarioDto
    {
        // Nombre completo del usuario.
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(150)]
        public string Nombre { get; set; } = string.Empty;

        // Correo electrónico del usuario.
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
        [StringLength(150)]
        public string Correo { get; set; } = string.Empty;

        // Contraseña que será convertida en un hash seguro.
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        public string Password { get; set; } = string.Empty;

        // Identificador del rol asignado.
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un rol válido.")]
        public int RolId { get; set; }
    }
}