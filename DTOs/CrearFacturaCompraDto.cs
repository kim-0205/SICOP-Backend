
using System.ComponentModel.DataAnnotations;

namespace SICOP.DTOs
{
    // Define los datos necesarios para registrar una factura.
    public class CrearFacturaCompraDto
    {
        // Identifica al proveedor de la factura.
        [Range(1, int.MaxValue,
            ErrorMessage = "Debe seleccionar un proveedor válido.")]
        public int ProveedorId { get; set; }

        // Número de factura obligatorio.
        [Required(ErrorMessage = "El número de factura es obligatorio.")]
        [StringLength(50)]
        public string NumeroFactura { get; set; } = string.Empty;

        // Fecha en que se emitió la factura.
        [Required(ErrorMessage = "La fecha de emisión es obligatoria.")]
        public DateTime? FechaEmision { get; set; }

        // Fecha límite para pagar la factura.
        [Required(ErrorMessage = "La fecha de vencimiento es obligatoria.")]
        public DateTime? FechaVencimiento { get; set; }

        // Monto total de la factura.
        [Range(typeof(decimal), "0.01", "9999999999999999.99",
            ErrorMessage = "El total debe ser mayor que cero.")]
        public decimal Total { get; set; }

        // Información adicional opcional.
        [StringLength(500)]
        public string? Observaciones { get; set; }
    }
}

