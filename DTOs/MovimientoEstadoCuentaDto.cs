namespace SICOP.DTOs
{
    // Representa un movimiento financiero
    // dentro del estado de cuenta de un proveedor.
    public class MovimientoEstadoCuentaDto
    {
        // Fecha en que ocurrió el movimiento.
        public DateTime Fecha { get; set; }

        // Identificador de la factura relacionada.
        public int FacturaCompraId { get; set; }

        // Identificador del pago, cuando corresponda.
        public int? PagoId { get; set; }

        // Número de la factura relacionada.
        public string NumeroFactura { get; set; } = string.Empty;

        // Tipo de movimiento: Factura o Pago.
        public string TipoMovimiento { get; set; } = string.Empty;

        // Descripción del movimiento.
        public string Descripcion { get; set; } = string.Empty;

        // Importe que aumenta la deuda del proveedor.
        public decimal Cargo { get; set; }

        // Importe que disminuye la deuda del proveedor.
        public decimal Abono { get; set; }

        // Saldo acumulado después del movimiento.
        public decimal SaldoAcumulado { get; set; }

        // Indica si el movimiento fue anulado.
        public bool Anulado { get; set; }

        // Motivo de anulación, cuando corresponda.
        public string? MotivoAnulacion { get; set; }
    }
}