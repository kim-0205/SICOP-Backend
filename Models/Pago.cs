
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SICOP.Models
{
    // Representa los pagos realizados a una factura.
    public class Pago
    {
        // Llave primaria del pago.
        [Key]
        public int PagoId { get; set; }

        // Llave foránea de la factura.
        [Required]
        public int FacturaCompraId { get; set; }

        // Fecha en que se realizó el pago.
        [Required]
        public DateTime FechaPago { get; set; }

        // Cantidad abonada a la factura.
        [Column(TypeName = "decimal(18,2)")]
        [Range(typeof(decimal), "0.01", "9999999999999999.99",
            ErrorMessage = "El monto debe ser mayor que cero.")]
        public decimal Monto { get; set; }

        // Método utilizado para realizar el pago.
        [Required(ErrorMessage = "El método de pago es obligatorio.")]
        [StringLength(50)]
        public string MetodoPago { get; set; } = string.Empty;

        // Número de referencia o comprobante.
        [StringLength(100)]
        public string? Referencia { get; set; }

        // Información adicional del pago.
        [StringLength(500)]
        public string? Observaciones { get; set; }

        // Relación: cada pago pertenece a una factura.
        [ForeignKey(nameof(FacturaCompraId))]
        public FacturaCompra FacturaCompra { get; set; } = null!;


        // Indica si el pago fue anulado.
        // false = Pago válido.
        // true = Pago anulado.
        public bool Anulado { get; set; } = false;

        // Fecha en que se realizó la anulación.
        public DateTime? FechaAnulacion { get; set; }

        // Motivo por el cual se anuló el pago.
        [StringLength(500)]
        public string? MotivoAnulacion { get; set; }

    }
}
