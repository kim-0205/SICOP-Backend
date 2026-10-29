using System.ComponentModel.DataAnnotations;

namespace SICOP.DTOs
{
    // Define los datos que pueden actualizarse de un proveedor.
    public class ActualizarProveedorDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El NIT es obligatorio.")]
        [StringLength(30)]
        public string NIT { get; set; } = string.Empty;

        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "El correo no es válido.")]
        public string? Correo { get; set; }

        public string? Direccion { get; set; }
    }
}