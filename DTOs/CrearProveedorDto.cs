using System.ComponentModel.DataAnnotations;

namespace SICOP.DTOs
{
    // Define los datos necesarios para registrar un proveedor.
    public class CrearProveedorDto
    {
        // Nombre obligatorio del proveedor.
        [Required(ErrorMessage = "El nombre del proveedor es obligatorio.")]
        [StringLength(150)]
        public string Nombre { get; set; } = string.Empty;

        // NIT obligatorio del proveedor.
        [Required(ErrorMessage = "El NIT es obligatorio.")]
        [StringLength(20)]
        public string NIT { get; set; } = string.Empty;

        // Número de teléfono.
        [StringLength(20)]
        public string? Telefono { get; set; }

        // Correo electrónico válido.
        [EmailAddress(ErrorMessage = "Debe ingresar un correo electrónico válido.")]
        public string? Correo { get; set; }

        // Dirección del proveedor.
        [StringLength(250)]
        public string? Direccion { get; set; }
    }
}