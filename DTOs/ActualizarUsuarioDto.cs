using System.ComponentModel.DataAnnotations;

namespace SICOP.DTOs
{
    // Define los datos que pueden actualizarse de un usuario.
    public class ActualizarUsuarioDto
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

        // Identificador del rol asignado.
        [Range(1, int.MaxValue,
            ErrorMessage = "Seleccione un rol válido.")]
        public int RolId { get; set; }
    }
}