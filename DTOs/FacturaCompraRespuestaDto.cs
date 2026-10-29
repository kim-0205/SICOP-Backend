
namespace SICOP.DTOs
{
    // Define los datos que la API devolverá
    // al registrar o consultar una factura.
    public class FacturaCompraRespuestaDto
    {
        // Identificador de la factura.
        public int FacturaCompraId { get; set; }

        // Identificador del proveedor.
        public int ProveedorId { get; set; }

        // Número de factura.
        public string NumeroFactura { get; set; } = string.Empty;

        // Fecha de emisión.
        public DateTime FechaEmision { get; set; }

        // Fecha límite de pago.
        public DateTime FechaVencimiento { get; set; }

        // Monto total de la factura.
        public decimal Total { get; set; }

        // Información adicional.
        public string? Observaciones { get; set; }
    }
}