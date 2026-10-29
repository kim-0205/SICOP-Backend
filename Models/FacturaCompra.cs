using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SICOP.Models
{
    // Representa una factura de compra registrada en SICOP.
    public class FacturaCompra
    {
        // Llave primaria de la factura.
        [Key]
        public int FacturaCompraId { get; set; }

        // Llave foránea que identifica al proveedor.
        [Required]
        public int ProveedorId { get; set; }

        // Número de factura obligatorio.
        [Required(ErrorMessage = "El número de factura es obligatorio.")]
        [StringLength(50)]
        public string NumeroFactura { get; set; } = string.Empty;

        // Fecha en que se emitió la factura.
        [Required]
        public DateTime FechaEmision { get; set; }

        // Fecha límite para realizar el pago.
        [Required]
        public DateTime FechaVencimiento { get; set; }

        // Importe total de la factura.
        [Column(TypeName = "decimal(18,2)")]
        [Range(typeof(decimal), "0.01", "9999999999999999.99",
            ErrorMessage = "El total debe ser mayor que cero.")]
        public decimal Total { get; set; }

        // Información adicional opcional.
        [StringLength(500)]
        public string? Observaciones { get; set; }


        // Indica si la factura fue anulada.
        public bool Anulada { get; set; } = false;

        // Fecha en que se anuló la factura.
        public DateTime? FechaAnulacion { get; set; }

        // Motivo por el cual se anuló la factura.
        [StringLength(500)]
        public string? MotivoAnulacion { get; set; }

        // Identificador del administrador que anuló la factura.
        public int? AnuladaPorUsuarioId { get; set; }


        // Relación: cada factura pertenece a un proveedor.
        [ForeignKey(nameof(ProveedorId))]
        public Proveedor? Proveedor { get; set; }

        // Relación: una factura puede tener varios pagos.
        public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    }
}