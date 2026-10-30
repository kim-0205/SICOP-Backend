
namespace SICOP.DTOs
{
    // Representa un pago dentro del reporte
    // de historial de pagos de SICOP.
    public class ReportePagoDto
    {
        // Identificador del pago.
        public int PagoId { get; set; }

        // Identificador de la factura relacionada.
        public int FacturaCompraId { get; set; }

        // Número de factura.
        public string NumeroFactura { get; set; } = string.Empty;

        // Identificador del proveedor.
        public int ProveedorId { get; set; }

        // Nombre del proveedor.
        public string NombreProveedor { get; set; } = string.Empty;

        // Fecha en que se realizó el pago.
        public DateTime FechaPago { get; set; }

        // Monto original del pago.
        public decimal Monto { get; set; }

        // Método utilizado para realizar el pago.
        public string MetodoPago { get; set; } = string.Empty;

        // Referencia o comprobante del pago.
        public string? Referencia { get; set; }

        // Observaciones registradas.
        public string? Observaciones { get; set; }

        // Indica si el pago fue anulado.
        public bool Anulado { get; set; }

        // Fecha de anulación, cuando corresponda.
        public DateTime? FechaAnulacion { get; set; }

        // Motivo por el cual se anuló.
        public string? MotivoAnulacion { get; set; }
    }
}
