
using System.ComponentModel.DataAnnotations;

namespace SICOP.Models
{
    // Representa a los proveedores registrados en el sistema.
    public class Proveedor
    {
        // Llave primaria del proveedor.
        [Key]
        public int ProveedorId { get; set; }

        // Nombre obligatorio del proveedor.
        [Required(ErrorMessage = "El nombre del proveedor es obligatorio.")]
        [StringLength(150)]
        public string Nombre { get; set; } = string.Empty;

        // Número de Identificación Tributaria.
        [Required(ErrorMessage = "El NIT es obligatorio.")]
        [StringLength(20)]
        public string NIT { get; set; } = string.Empty;

        // Información de contacto opcional.
        [StringLength(20)]
        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
        [StringLength(150)]
        public string? Correo { get; set; }

        [StringLength(250)]
        public string? Direccion { get; set; }

        // Indica si el proveedor está activo.
        public bool Activo { get; set; } = true;

        // Relación: un proveedor puede tener muchas facturas.
        public ICollection<FacturaCompra> Facturas { get; set; }
            = new List<FacturaCompra>();
    }
}
