namespace SICOP.DTOs
{
    // Representa la información de una factura
    // utilizada en los reportes de cuentas por pagar.
    public class ReporteFacturaDto
    {
        // Identificador de la factura.
        public int FacturaCompraId { get; set; }

        // Número de factura.
        public string NumeroFactura { get; set; } = string.Empty;

        // Identificador del proveedor.
        public int ProveedorId { get; set; }

        // Nombre del proveedor.
        public string NombreProveedor { get; set; } = string.Empty;

        // Fecha de emisión de la factura.
        public DateTime FechaEmision { get; set; }

        // Fecha límite de pago.
        public DateTime FechaVencimiento { get; set; }

        // Importe original de la factura.
        public decimal Total { get; set; }

        // Suma de los pagos válidos realizados.
        public decimal TotalPagado { get; set; }

        // Cantidad pendiente de pago.
        public decimal SaldoPendiente { get; set; }

        // Estado financiero: Pendiente, Parcial, Pagada o Vencida.
        public string Estado { get; set; } = string.Empty;

        // Cantidad de días de atraso.
        public int DiasVencidos { get; set; }
    }
}