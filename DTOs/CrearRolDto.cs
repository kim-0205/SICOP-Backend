using System.ComponentModel.DataAnnotations;

namespace SICOP.DTOs
{
    // Define los datos necesarios para registrar un rol.
    public class CrearRolDto
    {
        // El nombre del rol es obligatorio.
        [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty;

        // La descripción del rol es opcional.
        [StringLength(200)]
        public string? Descripcion { get; set; }
    }
}